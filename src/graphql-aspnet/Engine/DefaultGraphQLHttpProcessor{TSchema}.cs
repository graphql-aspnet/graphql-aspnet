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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Logging.Extensions;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.Exceptions;
    using GraphQL.AspNet.Web.Security;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Processes a single graphql query through the runtime. This class is NOT thread safe
    /// and should not be reused.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class DefaultGraphQLHttpProcessor<TSchema> : IGraphQLHttpProcessor<TSchema>
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

        private readonly IGraphEventLogger _logger;
        private readonly TSchema _schema;
        private readonly IGraphQLRuntime<TSchema> _runtime;
        private readonly IGraphQueryResponseWriter<TSchema> _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema" /> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema" />.</param>
        /// <param name="writer">The result writer capable of converting a <see cref="IGraphOperationResult" /> into a serialized payload
        /// for the given <typeparamref name="TSchema" />.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        public DefaultGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IGraphQueryResponseWriter<TSchema> writer,
            IGraphEventLogger logger = null)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _runtime = Validation.ThrowIfNullOrReturn(runtime, nameof(runtime));
            _writer = Validation.ThrowIfNullOrReturn(writer, nameof(writer));

            _logger = logger;
        }

        /// <inheritdoc />
        public virtual async Task Invoke(HttpContext context)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));

            GraphQueryData queryData;
            try
            {
                queryData = await this.ParseHttpContext();
            }
            catch (HttpContextParsingException ex)
            {
                await this.WriteStatusCodeResponse(ex.StatusCode, ex.Message, context.RequestAborted).ConfigureAwait(false);
                return;
            }

            await this.SubmitGraphQLQuery(queryData, context.RequestAborted).ConfigureAwait(false);
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
        protected virtual async Task<GraphQueryData> ParseHttpContext()
        {
            var dataGenerator = new HttpContextParser(this.HttpContext);
            return await dataGenerator.Parse();
        }

        /// <summary>
        /// Submits the request data to the GraphQL runtime for processing. When overloading in a child class, allows the class
        /// to interject and alter the <paramref name="queryData" /> just prior to it being executed by the graphql runtime.
        /// </summary>
        /// <param name="queryData">The query data parsed from an <see cref="HttpRequest" />; may be null.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IActionResult&gt;.</returns>
        public virtual async Task SubmitGraphQLQuery(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            // ensure data was received
            if (queryData == null || string.IsNullOrWhiteSpace(queryData.Query))
            {
                await this.WriteStatusCodeResponse(HttpStatusCode.BadRequest, ERROR_NO_QUERY_PROVIDED, cancelToken).ConfigureAwait(false);
                return;
            }

            await this.ExecuteGraphQLQuery(queryData, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the GraphQL query. When overriden in a child class allows the class to override the default behavior of
        /// processing a query against the GraphQL runtime and writing the result to the <see cref="HttpResponse" />.
        /// </summary>
        /// <param name="queryData">The query data.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IGraphOperationResult&gt;.</returns>
        protected virtual async Task ExecuteGraphQLQuery(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            try
            {
                // *******************************
                // Setup
                // *******************************
                var request = await this.CreateOperationRequest(queryData, cancelToken);
                if (request == null)
                {
                    await this.WriteStatusCodeResponse(HttpStatusCode.InternalServerError, ERROR_NO_REQUEST_CREATED, cancelToken).ConfigureAwait(false);
                    return;
                }

                this.GraphQLRequest = request;
                var securityContext = this.CreateUserSecurityContext();

                // *******************************
                // Primary query execution
                // *******************************
                var queryResponse = await _runtime
                    .ExecuteRequest(
                        this.HttpContext.RequestServices,
                        this.GraphQLRequest,
                        securityContext,
                        this.EnableMetrics,
                        cancelToken)
                    .ConfigureAwait(false);

                // if any metrics were populated in the execution, allow a child class to process them
                if (queryResponse.Metrics != null)
                    this.HandleQueryMetrics(queryResponse.Metrics);

                // all done, finalize and return
                queryResponse = this.FinalizeResult(queryResponse);
                await this.WriteResponse(queryResponse, cancelToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionResult = this.HandleQueryException(ex);
                if (exceptionResult == null)
                {
                    // no one was able to handle the exception?
                    // Log it if able and just fail out to the caller
                    if (_logger != null)
                    {
                        if (ex is AggregateException ae)
                        {
                            foreach (var internalException in ae.InnerExceptions)
                                _logger.UnhandledExceptionEvent(internalException);
                        }
                        else
                        {
                            _logger.UnhandledExceptionEvent(ex);
                        }
                    }

                    await this.WriteStatusCodeResponse(HttpStatusCode.InternalServerError, ERROR_INTERNAL_SERVER_ISSUE, cancelToken).ConfigureAwait(false);
                }
                else
                {
                    await this.WriteResponse(exceptionResult, cancelToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// When overriden in a child class, allows for updating or repackaging of the graphql operation
        /// request before it is sent to the runtime for processing.
        /// </summary>
        /// <param name="queryData">The query data that needs to be packaged into a <see cref="IGraphOperationRequest" />.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>IGraphOperationRequest.</returns>
        protected virtual async Task<IGraphOperationRequest> CreateOperationRequest(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            var request = _runtime.CreateRequest(queryData);
            if (request == null)
                await this.WriteStatusCodeResponse(HttpStatusCode.InternalServerError, ERROR_NO_REQUEST_CREATED, cancelToken).ConfigureAwait(false);

            // repackage the runtime request to carry the
            // HttpContext along. It's not used or needed by the runtime
            // but its useful within controller action method invocations
            return new GraphOperationWebRequest(request, this.HttpContext);
        }

        /// <summary>
        /// When overriden in a child class, allows for the creation of a custom security context
        /// to represent the user's supplied credentials to the graphql pipeline.
        /// </summary>
        /// <returns>IUserSecurityContext.</returns>
        protected virtual IUserSecurityContext CreateUserSecurityContext()
        {
            return new HttpUserSecurityContext(this.HttpContext);
        }

        /// <summary>
        /// Writes directly to the <see cref="HttpResponse" /> stream with the given status code
        /// and message.
        /// </summary>
        /// <param name="statusCode">The status code to deliver on the response.</param>
        /// <param name="message">The message to deliver with the given code.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected async Task WriteStatusCodeResponse(HttpStatusCode statusCode, string message, CancellationToken cancelToken = default)
        {
            if (_schema.Configuration.ResponseOptions.AppendServerHeader)
            {
                this.Response.Headers.Add(Constants.ServerInformation.SERVER_INFORMATION_HEADER, Constants.ServerInformation.ServerData);
            }

            this.Response.StatusCode = (int)statusCode;
            await this.Response.WriteAsync(message, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the given operation result directly to the output stream.
        /// </summary>
        /// <param name="result">The operation result to write.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task WriteResponse(IGraphOperationResult result, CancellationToken cancelToken = default)
        {
            this.Response.ContentType = Constants.MediaTypes.JSON;
            if (_schema.Configuration.ResponseOptions.AppendServerHeader)
            {
                this.Response.Headers.Add(Constants.ServerInformation.SERVER_INFORMATION_HEADER, Constants.ServerInformation.ServerData);
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
        /// by the execution of the query. If an <see cref="IGraphOperationResult"/> is returned from this method the runtime will return
        /// it as the graphql response.  If null is returned, a status 500 result will be generated with a generic error message.
        /// </para>
        /// </summary>
        /// <param name="exception">The exception that was thrown by the runtime, if any.</param>
        /// <returns>The result, if any, of handling the exception. Return null to allow default processing to occur.</returns>
        protected virtual IGraphOperationResult HandleQueryException(Exception exception)
        {
            return null;
        }

        /// <summary>
        /// When overriden in a child class, this method provides access to the metrics package populated during a query run to facilicate custom processing.
        /// This method is only called if a metrics package was generated for the request and will be invoked regardless of whether metrics are
        /// exposed to the requestor in a response package.
        /// </summary>
        /// <param name="metrics">The metrics containing information about the last run.</param>
        protected virtual void HandleQueryMetrics(IGraphQueryExecutionMetrics metrics)
        {
        }

        /// <summary>
        /// Generates a qualified <see cref="IGraphOperationRequest" /> with the given message
        /// wrapped as a graphql error allowing it to be processed
        /// by the client as a formatted, albeit errored, query response. When overridden in a child class this method
        /// allows the child to generate a custom <see cref="IGraphOperationResult" /> in response to the message.
        /// </summary>
        /// <param name="message">The error message to wrap.</param>
        /// <param name="errorCode">The error code to assign to the message.</param>
        /// <returns>IActionResult.</returns>
        protected virtual IGraphOperationResult ErrorMessageAsGraphQLResponse(
            string message,
            string errorCode = Constants.ErrorCodes.GENERAL_ERROR)
        {
            var response = new GraphOperationResult(this.GraphQLRequest);
            response.Messages.Add(GraphMessageSeverity.Critical, message, errorCode);
            return response;
        }

        /// <summary>
        /// Finalizes any result being sent to the requester by applying any invocation specific attributes
        /// to the outgoing object. This is the final processing step before the result is serialized and returned
        /// to the requester.
        /// </summary>
        /// <param name="result">The result generated froma query execution.</param>
        /// <returns>ExecutionResult.</returns>
        protected virtual IGraphOperationResult FinalizeResult(IGraphOperationResult result)
        {
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether this graphql request should enable and track internal metrics or not.
        /// </summary>
        /// <value><c>true</c> if metrics should be enabled, <c>false</c> otherwise.</value>
        protected virtual bool EnableMetrics => _schema.Configuration.ExecutionOptions.EnableMetrics;

        /// <summary>
        /// Gets a value indicating whether metrics, when enabled through <see cref="EnableMetrics"/>, are delivered to the user as
        /// part of the graphql result.
        /// </summary>
        /// <value><c>true</c> if metrics should be exposed to the requestor; otherwise, <c>false</c>.</value>
        protected virtual bool ExposeMetrics => _schema.Configuration.ResponseOptions.ExposeMetrics;

        /// <summary>
        /// Gets a value indicating whether this graphql request will expose exceptions to the requestor.
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed on the execution result, <c>false</c> otherwise.</value>
        protected virtual bool ExposeExceptions => _schema.Configuration.ResponseOptions.ExposeExceptions;

        /// <summary>
        /// Gets the claims principle representing the active, default user on the http context.
        /// </summary>
        /// <value>The user.</value>
        protected virtual ClaimsPrincipal User => this.HttpContext?.User;

        /// <summary>
        /// Gets the <see cref="HttpResponse"/> of the active <see cref="HttpContext"/>.
        /// </summary>
        /// <value>The response.</value>
        protected virtual HttpResponse Response => this.HttpContext?.Response;

        /// <summary>
        /// Gets the HTTP context assigned to this instance.
        /// </summary>
        /// <value>The HTTP context.</value>
        protected virtual HttpContext HttpContext { get; private set; }

        /// <summary>
        /// Gets the <see cref="HttpRequest"/> object of the active <see cref="HttpContext"/>.
        /// </summary>
        /// <value>The request.</value>
        protected virtual HttpRequest Request => this.HttpContext?.Request;

        /// <summary>
        /// Gets the GraphQL request that was created and processed. May not be populated until
        /// after <see cref="CreateOperationRequest"/> is called.
        /// </summary>
        /// <value>The graphQL request being executed by this processor.</value>
        protected virtual IGraphOperationRequest GraphQLRequest { get; private set; }
    }
}