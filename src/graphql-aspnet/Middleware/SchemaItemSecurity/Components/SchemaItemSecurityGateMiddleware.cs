// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SchemaItemSecurity.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A gate keeper at the front of the security pipeline to perform a quick exit
    /// for those fields that will never need security checks.
    /// </summary>
    public class SchemaItemSecurityGateMiddleware : ISchemaItemSecurityMiddleware
    {
        /// <inheritdoc />
        public async Task InvokeAsync(
            GraphSchemaItemSecurityChallengeContext context,
            GraphMiddlewareInvocationDelegate<GraphSchemaItemSecurityChallengeContext> next,
            CancellationToken cancelToken = default)
        {
            // if the target item absolutely does not require any auth checks
            // /just skip out on everything
            var doesItemRequiredAtuh =
                context?.SecureSchemaItem?.SecurityGroups?.HasSecurityChecks ?? true;

            if (!doesItemRequiredAtuh)
            {
                // security context can and will be null in situations where
                // security is not configured on the server
                context.Result = SchemaItemSecurityChallengeResult.Skipped(context.SecurityContext?.DefaultUser);
                return;
            }

            await next(context, cancelToken);
        }
    }
}