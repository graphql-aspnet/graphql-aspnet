// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web
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
    using GraphQL.AspNet.Web.Security;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A base class providing some functionality common to many <see cref="IGraphQLHttpProcessor{TSchema}"/>
    /// objects.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public abstract class GraphQLHttpProcessorBase<TSchema> : IGraphQLHttpProcessor<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLHttpProcessorBase{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema" /> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema" />.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        protected GraphQLHttpProcessorBase(
            ISchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IGraphEventLogger logger = null)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            this.Runtime = Validation.ThrowIfNullOrReturn(runtime, nameof(runtime));
            this.Logger = logger;
        }

        /// <inheritdoc />
        public abstract Task InvokeAsync(HttpContext context);

        /// <summary>
        /// Generates a qualified <see cref="IQueryExecutionResult" /> with the given message
        /// wrapped as a graphql error allowing it to be processed
        /// by the client as a formatted, albeit errored, query response. When overridden in a child class this method
        /// allows the child to generate a custom <see cref="IQueryExecutionResult" /> in response to the message.
        /// </summary>
        /// <param name="originalRequest">The request to act as the origin of the generated response.</param>
        /// <param name="message">The error message to wrap.</param>
        /// <param name="errorCode">The error code to assign to the message.</param>
        /// <returns>IActionResult.</returns>
        protected virtual IQueryExecutionResult ErrorMessageAsGraphQLResponse(
            IQueryExecutionRequest originalRequest,
            string message,
            string errorCode = Constants.ErrorCodes.GENERAL_ERROR)
        {
            return this.ExceptionAsGraphQLResponse(originalRequest, message, errorCode, null);
        }

        /// <summary>
        /// Generates a qualified <see cref="IQueryExecutionResult" /> with the given exception and friendly error message
        /// wrapped as a graphql error allowing it to be processed by the client as a formatted, albeit errored, query response.
        /// When overridden in a child class this method allows the child to generate a custom <see cref="IQueryExecutionResult" />
        /// in response to the message.
        /// </summary>
        /// <param name="originalRequest">The request to act as the origin of the generated response.</param>
        /// <param name="message">The error message to wrap.</param>
        /// <param name="errorCode">The error code to assign to the message.</param>
        /// <param name="exceptionThrown">An exception that was thrown and should be included in the response. This exception will
        /// only be exposed if the schema configuration allows it.</param>
        /// <returns>IActionResult.</returns>
        protected virtual IQueryExecutionResult ExceptionAsGraphQLResponse(
            IQueryExecutionRequest originalRequest,
            string message,
            string errorCode = Constants.ErrorCodes.GENERAL_ERROR,
            Exception exceptionThrown = null)
        {
            var response = new QueryExecutionResult(originalRequest);
            response.Messages.Add(GraphMessageSeverity.Critical, message, errorCode, exceptionThrown: exceptionThrown);
            return response;
        }

        /// <summary>
        /// Writes directly to the <see cref="HttpResponse" /> stream with the given status code
        /// and message.
        /// </summary>
        /// <param name="statusCode">The status code to deliver on the response.</param>
        /// <param name="message">The message to deliver with the given code.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected async Task WriteStatusCodeResponseAsync(HttpStatusCode statusCode, string message, CancellationToken cancelToken = default)
        {
            if (this.Response == null)
            {
                throw new InvalidOperationException("Unable to write to the response. No HttpContext has been " +
                    "defined for this instance");
            }

            if (this.Schema.Configuration.ResponseOptions.AppendServerHeader)
            {
                this.Response.Headers.Append(Constants.ServerInformation.SERVER_INFORMATION_HEADER, Constants.ServerInformation.ServerData);
            }

            this.Response.StatusCode = (int)statusCode;
            await this.Response.WriteAsync(message, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// When overriden in a child class, allows for alternate creation of the primary graphql operation
        /// request. The generated request is sent to the graphql runtime for processing
        /// and should NOT contain any errors. If an error does occur, return <c>null</c>,
        /// and an appropriate error response will be written to the response.
        /// </summary>
        /// <param name="queryData">The query data that needs to be packaged into a <see cref="IQueryExecutionRequest" />.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>IQueryExecutionRequest.</returns>
        protected virtual Task<IQueryExecutionRequest> CreateQueryRequestAsync(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            var request = this.Runtime.CreateRequest(queryData);
            if (request == null)
                return Task.FromResult(null as IQueryExecutionRequest);

            // repackage the runtime request to carry the
            // HttpContext along. It's not used or needed by the runtime
            // but its useful within controller action method invocations
            request = new QueryExecutionWebRequest(request, this.HttpContext);
            return Task.FromResult(request);
        }

        /// <summary>
        /// When overriden in a child class, allows for alternate creation of the security context
        /// that represent the user's supplied credentials to the graphql pipeline.
        /// </summary>
        /// <returns>IUserSecurityContext.</returns>
        protected virtual IUserSecurityContext CreateUserSecurityContext()
        {
            return new HttpUserSecurityContext(this.HttpContext);
        }

        /// <summary>
        /// Gets the schema instance this query is being processed against.
        /// </summary>
        /// <value>The schema.</value>
        protected ISchema Schema { get; }

        /// <summary>
        /// Gets the runtime instance this processor will execute queries against.
        /// </summary>
        /// <value>The runtime instance.</value>
        protected IGraphQLRuntime<TSchema> Runtime { get; }

        /// <summary>
        /// Gets the logger instance this processor will write any log messages to.
        /// </summary>
        /// <value>The logger instance used by this processor.</value>
        protected IGraphEventLogger Logger { get; }

        /// <summary>
        /// Gets a value indicating whether this graphql request should enable and track internal metrics or not.
        /// </summary>
        /// <value><c>true</c> if metrics should be enabled, <c>false</c> otherwise.</value>
        protected virtual bool EnableMetrics => this.Schema.Configuration.ExecutionOptions.EnableMetrics;

        /// <summary>
        /// Gets a value indicating whether metrics, when enabled through <see cref="EnableMetrics"/>, are delivered to the user as
        /// part of the graphql result.
        /// </summary>
        /// <value><c>true</c> if metrics should be exposed to the requestor; otherwise, <c>false</c>.</value>
        protected virtual bool ExposeMetrics => this.Schema.Configuration.ResponseOptions.ExposeMetrics;

        /// <summary>
        /// Gets a value indicating whether this graphql request will expose exceptions to the requestor.
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed on the execution result, <c>false</c> otherwise.</value>
        protected virtual bool ExposeExceptions => this.Schema.Configuration.ResponseOptions.ExposeExceptions;

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
        /// Gets or sets the HTTP context assigned to this instance.
        /// </summary>
        /// <remarks>
        /// This property may be null if accessed prior to calling Invoke.
        /// </remarks>
        /// <value>The HTTP context.</value>
        protected virtual HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets the <see cref="HttpRequest"/> object of the active <see cref="HttpContext"/>.
        /// </summary>
        /// <value>The request.</value>
        protected virtual HttpRequest Request => this.HttpContext?.Request;
    }
}