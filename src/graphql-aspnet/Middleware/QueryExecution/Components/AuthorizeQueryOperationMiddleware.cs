// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.QueryExecution.Components
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// When part of a pipeline, this component will pre-authorize every field of the operation or fail the context
    /// if authorization fails.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware exists for.</typeparam>
    public class AuthorizeQueryOperationMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipeline<TSchema, GraphSchemaItemSecurityChallengeContext> _authPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeQueryOperationMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="authPipeline">The authentication pipeline.</param>
        public AuthorizeQueryOperationMiddleware(ISchemaPipeline<TSchema, GraphSchemaItemSecurityChallengeContext> authPipeline)
        {
            _authPipeline = Validation.ThrowIfNullOrReturn(authPipeline, nameof(authPipeline));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.Operation != null)
            {
                var isAuthorized = await this.AuthorizeOperation(context, cancelToken).ConfigureAwait(false);
                if (!isAuthorized)
                    context.Cancel();
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Iterates over every secure field in the operation on the context, attempting to authorize the
        /// user to each one.
        /// </summary>
        /// <param name="context">The primary query context.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns><c>true</c> if authorization was successful, otherwise false.</returns>
        private async Task<bool> AuthorizeOperation(GraphQueryExecutionContext context, CancellationToken cancelToken)
        {
            var authTasks = new List<Task>();
            bool isAuthorized = true;
            foreach (var securePart in context.Operation.SecureItems)
            {
                // should be caught by validation but just in case prevent an auth
                if (securePart?.SecureItem == null)
                    continue;

                var authRequest = new GraphSchemaItemSecurityRequest(securePart);
                var authContext = new GraphSchemaItemSecurityChallengeContext(context, authRequest);

                var pipelineTask = _authPipeline.InvokeAsync(authContext, cancelToken)
                    .ContinueWith(
                        (_) =>
                        {
                            var authResult = authContext.Result ?? SchemaItemSecurityChallengeResult.Default();

                            // fake the path elements from the field route. since we don't have a full resolution chain
                            // when doing query level authorization (no indexers on potential child fields since
                            // nothing is actually resolved yet)
                            if (!authResult.Status.IsAuthorized())
                            {
                                context.Messages.Critical(
                                    $"Access Denied to {securePart.SecureItem.Route.Path}",
                                    Constants.ErrorCodes.ACCESS_DENIED,
                                    securePart.SourceLocation.AsOrigin());
                                isAuthorized = false;
                            }
                        }, cancelToken);

                authTasks.Add(pipelineTask);
            }

            await Task.WhenAll(authTasks).ConfigureAwait(false);
            return isAuthorized;
        }
    }
}