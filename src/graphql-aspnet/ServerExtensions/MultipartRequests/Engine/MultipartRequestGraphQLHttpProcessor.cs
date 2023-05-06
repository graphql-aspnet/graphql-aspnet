// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.Exceptions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A custom http query processor that supports processing http requests conforming to the
    /// <c>graphql-multipart-request</c> specification.
    /// Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec"/>.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor will work with.</typeparam>
    public class MultipartRequestGraphQLHttpProcessor<TSchema> : GraphQLHttpProcessorBase<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// An error message constant, in english, providing the text to return to the caller when a 500 error is generated.
        /// </summary>
        public const string ERROR_INTERNAL_SERVER_ISSUE = "Unknown internal server error. See exception for details.";

        /// <summary>
        /// An error message constant, in english, providing the text  to return to the caller when no operation could be created
        /// from the supplied data on the request.
        /// </summary>
        public const string ERROR_NO_REQUEST_CREATED = "The GraphQL Operation at index {0} is null. Unable to execute the query.";

        private readonly object _contextLocker = new object();
        private readonly IMultiPartHttpFormPayloadParser<TSchema> _parser;
        private readonly IQueryResponseWriter<TSchema> _writer;
        private IUserSecurityContext _securityContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestGraphQLHttpProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema" /> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema" />.</param>
        /// <param name="writer">The result writer capable of converting a <see cref="T:GraphQL.AspNet.Interfaces.Execution.IQueryExecutionResult" /> into a serialized payload
        /// for the given <typeparamref name="TSchema" />.</param>
        /// <param name="parser">The http context parser that will split the httpcontext
        /// according to the multi-part-request specification.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        public MultipartRequestGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IQueryResponseWriter<TSchema> writer,
            IMultiPartHttpFormPayloadParser<TSchema> parser,
            IGraphEventLogger logger = null)
            : base(schema, runtime, logger)
        {
            _parser = Validation.ThrowIfNullOrReturn(parser, nameof(parser));
            _writer = Validation.ThrowIfNullOrReturn(writer, nameof(writer));
        }

        /// <inheritdoc />
        public override async Task InvokeAsync(HttpContext context)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));

            MultiPartRequestGraphQLPayload payload;

            try
            {
                payload = await this.ParseHttpContextAsync();
            }
            catch (HttpContextParsingException ex)
            {
                await this.WriteStatusCodeResponseAsync(ex.StatusCode, ex.Message, context.RequestAborted).ConfigureAwait(false);
                return;
            }

            await this.SubmitMultipartQueryAsync(payload, context.RequestAborted).ConfigureAwait(false);
        }

        /// <summary>
        /// Submits the request batch to the GraphQL runtime for processing.
        /// </summary>
        /// <param name="payload">The query data payload parsed from an <see cref="HttpRequest" />; may be null.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task SubmitMultipartQueryAsync(MultiPartRequestGraphQLPayload payload, CancellationToken cancelToken = default)
        {
            var requests = new List<IQueryExecutionRequest>();
            var individualQueryTasks = new List<Task<IQueryExecutionResult>>();
            List<IQueryExecutionResult> results = null;

            if (payload == null || payload.Count == 0)
            {
                await this.WriteStatusCodeResponseAsync(
                    HttpStatusCode.BadRequest,
                    "Invalid multi-part request payload.",
                    cancelToken);
                return;
            }

            // Fire off all items in the batch so they can start processing.
            for (var i = 0; i < payload.Count; i++)
            {
                var request = await this.CreateQueryRequestAsync(payload[i], cancelToken);
                var queryTask = this.SubmitSingleQueryAsync(i, request, cancelToken);

                requests.Add(request);
                individualQueryTasks.Add(queryTask);
            }

            // complete all tasks
            await Task.WhenAll(individualQueryTasks);

            // gather results in the order they queries were submitted for processing
            results = new List<IQueryExecutionResult>(individualQueryTasks.Count);
            for (var i = 0; i < individualQueryTasks.Count; i++)
            {
                var result = this.PackageBatchTaskAsResult(
                    i,
                    requests[i],
                    individualQueryTasks[i]);

                results.Add(result);
            }

            // write teh batch response to the requestor
            await this.WriteQueryResponseAsync(
                results,
                payload.IsBatch,
                cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Packages a resulted <see cref="Task" />, representing single query within a batch, into a final result object
        /// for serialization to the caller.
        /// </summary>
        /// <param name="index">The index number of the query in the batch that is being packaged.</param>
        /// <param name="request">The original request that was processed through the <paramref name="batchTask"/>.</param>
        /// <param name="batchTask">A completed task representing one member of a batch.</param>
        /// <returns>MultiPartRequestGraphQLResult.</returns>
        protected virtual IQueryExecutionResult PackageBatchTaskAsResult(
            int index,
            IQueryExecutionRequest request,
            Task<IQueryExecutionResult> batchTask)
        {
            Validation.ThrowIfNull(batchTask, nameof(batchTask));
            if (batchTask.IsCompleted && batchTask.Result != null)
                return batchTask.Result;

            IQueryExecutionResult errorResult = null;
            if (!batchTask.IsCompleted)
            {
                errorResult = this.ErrorMessageAsGraphQLResponse(
                    request,
                    $"Incomplete operation. The task assigned to complete the query at index {index} of the " +
                    "supplied batch had not yet been completed when a final result was requested by " +
                    "the http processor. The final state of the query is unknown.");
            }
            else if (batchTask.Exception != null)
            {
                errorResult = this.ExceptionAsGraphQLResponse(
                    request,
                    "Failed operation. The task assigned to complete the " +
                    $"query at index {index} of the supplied batch failed. See Exception for details.",
                    exceptionThrown: batchTask.UnwrapException());
            }

            errorResult = errorResult ?? this.ErrorMessageAsGraphQLResponse(
                request,
                $"Unknown operation status. The task assigned to complete the query at index {index} of the " +
                "supplied batch completed but did not provide a result nor did it indicate that an error occured. " +
                "The final state of the query is unknown.");

            return errorResult;
        }

        /// <summary>
        /// Executes a single GraphQL query and produces a result that can be merged into
        /// a pending multi-part batch request. When overriden in a child class allows the class to override the default behavior of
        /// processing a query against the GraphQL runtime.
        /// </summary>
        /// <param name="index">The index of the query within the batch.</param>
        /// <param name="request">A properly formatted graphql request to process against the runtime.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task<IQueryExecutionResult> SubmitSingleQueryAsync(
            int index,
            IQueryExecutionRequest request,
            CancellationToken cancelToken = default)
        {
            try
            {
                if (request == null)
                {
                    return this.ErrorMessageAsGraphQLResponse(
                            null,
                            string.Format(ERROR_NO_REQUEST_CREATED, index));
                }

                var securityContext = this.CreateUserSecurityContext();

                // *******************************
                // Primary query execution
                // *******************************
                var queryResponse = await this.Runtime
                    .ExecuteRequestAsync(
                        this.HttpContext.RequestServices,
                        request,
                        securityContext,
                        this.EnableMetrics,
                        cancelToken)
                    .ConfigureAwait(false);

                // if any metrics were populated in the execution, allow a child class to process them
                if (queryResponse.Metrics != null)
                    this.HandleQueryMetrics(index, queryResponse.Metrics);

                // all done, finalize and return
                queryResponse = this.FinalizeResult(index, queryResponse);
                return queryResponse;
            }
            catch (Exception ex)
            {
                var exceptionResult = this.HandleQueryException(index, ex);
                if (exceptionResult == null)
                {
                    // no one was able to handle the exception?
                    // Log it if able and just fail out to the caller
                    if (this.Logger != null)
                    {
                        if (ex is AggregateException ae)
                        {
                            foreach (var internalException in ae.InnerExceptions)
                                this.Logger.UnhandledExceptionEvent(internalException);
                        }
                        else
                        {
                            this.Logger.UnhandledExceptionEvent(ex);
                        }
                    }

                    exceptionResult = this.ExceptionAsGraphQLResponse(
                        request,
                        ERROR_INTERNAL_SERVER_ISSUE,
                        exceptionThrown: ex);
                }

                return exceptionResult;
            }
        }

        /// <summary>
        /// When overriden in a child class, allows for altering the way an operation result
        /// is written to the response stream.
        /// </summary>
        /// <param name="results">The collection of all the results built in this request.</param>
        /// <param name="renderAsBatch">if set to <c>true</c>, treats the results as a batch,
        /// rendering it as an array of result object rather than just a single item.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task WriteQueryResponseAsync(
            IReadOnlyList<IQueryExecutionResult> results,
            bool renderAsBatch,
            CancellationToken cancelToken = default)
        {
            this.Response.ContentType = Constants.MediaTypes.JSON;
            if (this.Schema.Configuration.ResponseOptions.AppendServerHeader)
            {
                this.Response.Headers.Add(Constants.ServerInformation.SERVER_INFORMATION_HEADER, Constants.ServerInformation.ServerData);
            }

            var localWriter = renderAsBatch
                ? new BatchGraphQLHttpResponseWriter(
                    this.Schema,
                    results,
                    _writer)
                : new BatchGraphQLHttpResponseWriter(
                    this.Schema,
                    results[0],
                    _writer);

            await localWriter.WriteResultAsync(this.HttpContext, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// When overriden in a child class, this method provides access to the metrics package populated during a query run to facilicate custom processing.
        /// This method is only called if a metrics package was generated for the request and will be invoked regardless of whether metrics are
        /// exposed to the requestor in a response package.
        /// </summary>
        /// <param name="index">The index number of the query in the batch that produced the metrics.</param>
        /// <param name="metrics">The metrics containing information about the last run.</param>
        protected virtual void HandleQueryMetrics(int index, IQueryExecutionMetrics metrics)
        {
        }

        /// <summary>
        /// When overriden in a child class, allows for performing any final operations against a query result
        /// being sent to the requester by applying any invocation specific attributes to the
        /// outgoing object. This is the final processing step before the result is serialized and returned
        /// to the requester.
        /// </summary>
        /// <param name="index">The index number of the query in the batch that produced the result.</param>
        /// <param name="result">The result generated froma query execution.</param>
        /// <returns>The altered result.</returns>
        protected virtual IQueryExecutionResult FinalizeResult(int index, IQueryExecutionResult result)
        {
            return result;
        }

        /// <summary>
        /// <para>When overridden in a child class, provides the option to intercept an unhandled exception thrown
        /// by the execution of the query. If an <see cref="IQueryExecutionResult"/> is returned from this method the runtime will return
        /// it as the graphql response.  If null is returned, a status 500 result will be generated with a generic error message.
        /// </para>
        /// </summary>
        /// <param name="index">The index number of the query in the batch that threw the exception.</param>
        /// <param name="exception">The exception that was thrown by the runtime.</param>
        /// <returns>The result, if any, of handling the exception. Return null to allow default processing to occur.</returns>
        protected virtual IQueryExecutionResult HandleQueryException(int index, Exception exception)
        {
            return null;
        }

        /// <summary>
        /// When overriden in a child class, allows for the alteration of the method by which the various query
        /// parameters are extracted from the <see cref="HttpContext"/> for input to the graphql runtime.
        /// </summary>
        /// <remarks>
        /// Throw an <see cref="HttpContextParsingException"/> to stop execution and quickly write
        /// an error back to the requestor.
        /// </remarks>
        /// <returns>A parsed query data object containing the input parameters for the
        /// graphql runtime or <c>null</c>.</returns>
        protected virtual async Task<MultiPartRequestGraphQLPayload> ParseHttpContextAsync()
        {
            return await _parser.ParseAsync(this.HttpContext);
        }

        /// <inheritdoc />
        protected override IUserSecurityContext CreateUserSecurityContext()
        {
            // ensure only one security context exists and is used
            // for all batch query members.
            if (_securityContext == null)
            {
                lock (_contextLocker)
                {
                    if (_securityContext == null)
                        _securityContext = base.CreateUserSecurityContext();
                }
            }

            return _securityContext;
        }
    }
}