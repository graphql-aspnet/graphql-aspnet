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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// When part of a pipeline, this component will pre-authorize every field of the operation or fail the context
    /// if authorization fails.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware exists for.</typeparam>
    public class AuthorizeQueryOperationMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipeline<TSchema, SchemaItemSecurityChallengeContext> _authPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeQueryOperationMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="authPipeline">The authentication pipeline.</param>
        public AuthorizeQueryOperationMiddleware(ISchemaPipeline<TSchema, SchemaItemSecurityChallengeContext> authPipeline)
        {
            _authPipeline = Validation.ThrowIfNullOrReturn(authPipeline, nameof(authPipeline));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.Operation != null)
            {
                var isAuthorized = await this.AuthorizeOperationAsync(context, cancelToken).ConfigureAwait(false);
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
        private async Task<bool> AuthorizeOperationAsync(QueryExecutionContext context, CancellationToken cancelToken)
        {
            var authTasks = new List<Task>();
            bool isAuthorized = true;
            for (var i = 0; i < context.Operation.SecureItems.Count; i++)
            {
                var securePart = context.Operation.SecureItems[i];

                // should be caught by validation but just in case prevent an auth
                if (securePart?.SecureItem == null)
                    continue;

                var authRequest = new SchemaItemSecurityRequest(securePart);
                var authContext = new SchemaItemSecurityChallengeContext(context, authRequest);

                var pipelineTask = _authPipeline.InvokeAsync(authContext, cancelToken)
                    .ContinueWith(
                        (_) =>
                        {
                            var authResult = authContext.Result ?? SchemaItemSecurityChallengeResult.Default();

                            // fake the path elements from the field item path. since we don't have a full resolution chain
                            // when doing query level authorization (no indexers on potential child fields since
                            // nothing is actually resolved yet)
                            if (!authResult.Status.IsAuthorized())
                            {
                                context.Messages.Critical(
                                    $"Access Denied to {securePart.SecureItem.ItemPath.Path}",
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