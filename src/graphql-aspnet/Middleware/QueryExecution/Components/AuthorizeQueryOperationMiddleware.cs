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
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldAuthorization;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// When part of a pipeline, this component will pre-authorize every field of the operation or fail the context
    /// if authorization fails.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware exists for.</typeparam>
    public class AuthorizeQueryOperationMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipeline<TSchema, GraphFieldAuthorizationContext> _authPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeQueryOperationMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="authPipeline">The authentication pipeline.</param>
        public AuthorizeQueryOperationMiddleware(ISchemaPipeline<TSchema, GraphFieldAuthorizationContext> authPipeline)
        {
            _authPipeline = Validation.ThrowIfNullOrReturn(authPipeline, nameof(authPipeline));
        }

        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.QueryOperation != null)
            {
                var anyFieldFailed = await this.AuthorizeOperation(context, cancelToken).ConfigureAwait(false);
                if (anyFieldFailed)
                {
                    context.Cancel();
                }
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
            bool anyFieldFailed = false;
            foreach (var fieldContext in context.QueryOperation.SecureFieldContexts)
            {
                var authRequest = new GraphFieldAuthorizationRequest(fieldContext);
                var authContext = new GraphFieldAuthorizationContext(context, authRequest);

                var pipelineTask = _authPipeline.InvokeAsync(authContext, cancelToken)
                    .ContinueWith(
                        (_) =>
                        {
                            var authResult = authContext.Result ?? FieldAuthorizationResult.Default();

                            // fake the path elements from the field route. since we don't have a full resolution chain
                            // when doing query level authorization (no indexers on potential child fields since
                            // nothing is actually resolved yet)
                            if (!authResult.Status.IsAuthorized())
                            {
                                context.Messages.Critical(
                                    $"Access Denied to field {fieldContext.Field.Route.Path}",
                                    Constants.ErrorCodes.ACCESS_DENIED,
                                    fieldContext.Origin);
                                anyFieldFailed = true;
                            }
                        }, cancelToken);

                authTasks.Add(pipelineTask);
            }

            await Task.WhenAll(authTasks).ConfigureAwait(false);
            return anyFieldFailed;
        }
    }
}