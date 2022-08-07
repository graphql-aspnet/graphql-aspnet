// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.DirectiveExecution.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A piece of middleware that will evaluate the security policies attached to the directive on the context
    /// and ensure the user is granted access via said policies.
    /// </summary>
    /// <typeparam name="TSchema">The schema this middleware instance is registered for.</typeparam>
    public class AuthorizeDirectiveMiddleware<TSchema> : IDirectiveExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipeline<TSchema, GraphSchemaItemSecurityContext> _authPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeDirectiveMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="authPipeline">The authentication pipeline.</param>
        public AuthorizeDirectiveMiddleware(ISchemaPipeline<TSchema, GraphSchemaItemSecurityContext> authPipeline)
        {
            _authPipeline = Validation.ThrowIfNullOrReturn(authPipeline, nameof(authPipeline));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(GraphDirectiveExecutionContext context, GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> next, CancellationToken cancelToken)
        {
            SchemaItemSecurityChallengeResult result = SchemaItemSecurityChallengeResult.Default();
            if (context.IsValid)
            {
                // execute the authorization pipeline
                var authRequest = new GraphSchemaItemSecurityRequest(context.Request);
                var authContext = new GraphSchemaItemSecurityContext(context, authRequest);
                await _authPipeline.InvokeAsync(authContext, cancelToken).ConfigureAwait(false);

                result = authContext.Result ?? SchemaItemSecurityChallengeResult.Default();

                // by default, deny any stati not explicitly declared as "successful" by this component.
                if (result.Status.IsAuthorized())
                {
                    context.User = result.User;
                }
                else
                {
                    context.Messages.Critical(
                        $"Access Denied to directive {context.Directive.Route.Path}",
                        Constants.ErrorCodes.ACCESS_DENIED,
                        context.Request.Origin);

                    context.Cancel();
                }
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }
    }
}