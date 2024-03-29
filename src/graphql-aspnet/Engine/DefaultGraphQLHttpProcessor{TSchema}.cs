﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.Exceptions;
    using GraphQL.AspNet.Web.Security;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// <para>
    /// Processes a single graphql query, as part of an <see cref="HttpContext"/>, through the
    /// runtime.
    /// </para>
    /// <para>
    /// This class is NOT thread safe and should not be reused.
    /// </para>
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class DefaultGraphQLHttpProcessor<TSchema> : GraphQLHttpProcessorBase<TSchema>
            where TSchema : class, ISchema
    {
        /// <summary>
        /// An error message constant, in english, providing the text to return to the caller when no query data was present.
        /// </summary>
        protected const string ERROR_NO_QUERY_PROVIDED = "No query received on the request";

        /// <summary>
        /// An error message constant, in english, providing the text to return to the caller when a 500 error is generated.
        /// </summary>
        protected const string ERROR_INTERNAL_SERVER_ISSUE = "Unknown internal server error.";

        /// <summary>
        /// An error message constant, in english, providing the text  to return to the caller when no operation could be created
        /// from the supplied data on the request.
        /// </summary>
        protected const string ERROR_NO_REQUEST_CREATED = "GraphQL Operation Request is null. Unable to execute the query.";

        private readonly IQueryResponseWriter<TSchema> _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema" /> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema" />.</param>
        /// <param name="writer">The result writer capable of converting a <see cref="IQueryExecutionResult" /> into a serialized payload
        /// for the given <typeparamref name="TSchema" />.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        public DefaultGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IQueryResponseWriter<TSchema> writer,
            IGraphEventLogger logger = null)
            : base(schema, runtime, logger)
        {
            _writer = Validation.ThrowIfNullOrReturn(writer, nameof(writer));
        }

        /// <inheritdoc />
        public override async Task InvokeAsync(HttpContext context)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));

            GraphQueryData queryData;
            try
            {
                queryData = await this.ParseHttpContextAsync();
            }
            catch (HttpContextParsingException ex)
            {
                await this.WriteStatusCodeResponseAsync(ex.StatusCode, ex.Message, context.RequestAborted).ConfigureAwait(false);
                return;
            }

            await this.SubmitQueryAsync(queryData, context.RequestAborted).ConfigureAwait(false);
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
        protected virtual async Task<GraphQueryData> ParseHttpContextAsync()
        {
            var dataGenerator = new GraphQLHttpPayloadParser(this.HttpContext);
            return await dataGenerator.ParseAsync();
        }

        /// <summary>
        /// Submits the request data to the GraphQL runtime for processing. When overloading in a child class, allows the class
        /// to interject and alter the <paramref name="queryData" /> just prior to it being executed by the graphql runtime.
        /// </summary>
        /// <param name="queryData">The query data parsed from an <see cref="HttpRequest" />; may be null.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task SubmitQueryAsync(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            // ensure data was received
            if (queryData == null || string.IsNullOrWhiteSpace(queryData.Query))
            {
                await this.WriteStatusCodeResponseAsync(HttpStatusCode.BadRequest, ERROR_NO_QUERY_PROVIDED, cancelToken).ConfigureAwait(false);
                return;
            }

            await this.ExecuteQueryAsync(queryData, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the GraphQL query. When overriden in a child class allows the class to override the default behavior of
        /// processing a query against the GraphQL runtime and writing the result to the <see cref="HttpResponse" />.
        /// </summary>
        /// <param name="queryData">The query data.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task ExecuteQueryAsync(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            try
            {
                // *******************************
                // Setup
                // *******************************
                var request = await this.CreateQueryRequestAsync(queryData, cancelToken);
                if (request == null)
                {
                    await this.WriteStatusCodeResponseAsync(HttpStatusCode.InternalServerError, ERROR_NO_REQUEST_CREATED, cancelToken).ConfigureAwait(false);
                    return;
                }

                this.GraphQLQueryRequest = request;
                var securityContext = this.CreateUserSecurityContext();

                // *******************************
                // Primary query execution
                // *******************************
                var queryResponse = await this.Runtime
                    .ExecuteRequestAsync(
                        this.HttpContext.RequestServices,
                        this.GraphQLQueryRequest,
                        securityContext,
                        this.EnableMetrics,
                        cancelToken)
                    .ConfigureAwait(false);

                // if any metrics were populated in the execution, allow a child class to process them
                if (queryResponse.Metrics != null)
                    this.HandleQueryMetrics(queryResponse.Metrics);

                // all done, finalize and return
                queryResponse = this.FinalizeResult(queryResponse);
                await this.WriteResponseAsync(queryResponse, cancelToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionResult = this.HandleQueryException(ex);
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

                    await this.WriteStatusCodeResponseAsync(HttpStatusCode.InternalServerError, ERROR_INTERNAL_SERVER_ISSUE, cancelToken).ConfigureAwait(false);
                }
                else
                {
                    await this.WriteResponseAsync(exceptionResult, cancelToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// When overriden in a child class, allows for altering the way an operation result
        /// is written to the response stream.
        /// </summary>
        /// <param name="result">The operation result to write.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task WriteResponseAsync(IQueryExecutionResult result, CancellationToken cancelToken = default)
        {
            this.Response.ContentType = Constants.MediaTypes.JSON;
            if (this.Schema.Configuration.ResponseOptions.AppendServerHeader)
            {
                this.Response.Headers.Append(Constants.ServerInformation.SERVER_INFORMATION_HEADER, Constants.ServerInformation.ServerData);
            }

            var localWriter = new GraphQLHttpResponseWriter(
                result,
                _writer,
                this.ExposeMetrics,
                this.ExposeExceptions);

            await localWriter.WriteResultAsync(this.HttpContext, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// <para>When overridden in a child class, provides the option to intercept an unhandled exception thrown
        /// by the execution of the query. If an <see cref="IQueryExecutionResult"/> is returned from this method the runtime will return
        /// it as the graphql response.  If null is returned, a status 500 result will be generated with a generic error message.
        /// </para>
        /// </summary>
        /// <param name="exception">The exception that was thrown by the runtime.</param>
        /// <returns>The result, if any, of handling the exception. Return null to allow default processing to occur.</returns>
        protected virtual IQueryExecutionResult HandleQueryException(Exception exception)
        {
            return null;
        }

        /// <summary>
        /// When overriden in a child class, this method provides access to the metrics package populated during a query run to facilicate custom processing.
        /// This method is only called if a metrics package was generated for the request and will be invoked regardless of whether metrics are
        /// exposed to the requestor in a response package.
        /// </summary>
        /// <param name="metrics">The metrics containing information about the last run.</param>
        protected virtual void HandleQueryMetrics(IQueryExecutionMetrics metrics)
        {
        }

        /// <summary>
        /// Generates a qualified <see cref="IQueryExecutionResult" /> with the given message
        /// wrapped as a graphql error allowing it to be processed
        /// by the client as a formatted, albeit errored, query response. When overridden in a child class this method
        /// allows the child to generate a custom <see cref="IQueryExecutionResult" /> in response to the message.
        /// </summary>
        /// <param name="message">The error message to wrap.</param>
        /// <param name="errorCode">The error code to assign to the message.</param>
        /// <returns>IActionResult.</returns>
        protected virtual IQueryExecutionResult ErrorMessageAsGraphQLResponse(
            string message,
            string errorCode = Constants.ErrorCodes.GENERAL_ERROR)
        {
            return this.ErrorMessageAsGraphQLResponse(this.GraphQLQueryRequest, message, errorCode);
        }

        /// <summary>
        /// When overriden in a child class, allows for performing any final operations against a query result
        /// being sent to the requester by applying any invocation specific attributes to the
        /// outgoing object. This is the final processing step before the result is serialized and returned
        /// to the requester.
        /// </summary>
        /// <param name="result">The result generated froma query execution.</param>
        /// <returns>The altered result.</returns>
        protected virtual IQueryExecutionResult FinalizeResult(IQueryExecutionResult result)
        {
            return result;
        }

        /// <summary>
        /// Gets the GraphQL request that was created and processed.
        /// </summary>
        /// <remarks>
        /// This property may not be populated until after <see cref="GraphQLHttpProcessorBase{TSchema}.CreateQueryRequestAsync"/> is called.
        /// </remarks>
        /// <value>The graphQL request being executed by this processor.</value>
        protected virtual IQueryExecutionRequest GraphQLQueryRequest { get; private set; }
    }
}