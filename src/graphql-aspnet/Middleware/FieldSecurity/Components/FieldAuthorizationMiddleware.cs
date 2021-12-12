// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldSecurity.Components
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A piece of middleware, on the authorization pipeline, that can successfuly authorize a single user
    /// to a single field of data.
    /// </summary>
    internal class FieldAuthorizationMiddleware : IGraphFieldSecurityMiddleware
    {
        private readonly IAuthorizationService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthorizationMiddleware" /> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public FieldAuthorizationMiddleware(IAuthorizationService authService = null)
        {
            _authService = authService;
        }

        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphFieldSecurityContext context, GraphMiddlewareInvocationDelegate<GraphFieldSecurityContext> next, CancellationToken cancelToken)
        {
            context.Logger?.FieldAuthorizationChallenge(context);

            // a result may have been set by other middleware
            // in this auth pipeline, if a result is already determined just skip this step
            if (context.Result == null)
            {
                var result = await this.AuthorizeRequest(context.Request, context.AuthenticatedUser).ConfigureAwait(false);
                context.Result = result ?? FieldSecurityChallengeResult.Default();
            }

            context.Logger?.FieldAuthorizationChallengeResult(context);

            await next(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to authorize the request using the field based <see cref="IAuthorizeData" /> attributes.
        /// </summary>
        /// <param name="authRequest">The request to authorize.</param>
        /// <param name="claimsUser">The claims user to authorize against.</param>
        /// <returns>FieldSecurityChallengeResult.</returns>
        private async Task<FieldSecurityChallengeResult> AuthorizeRequest(
            IGraphFieldSecurityRequest authRequest,
            ClaimsPrincipal claimsUser)
        {
            Validation.ThrowIfNull(authRequest?.Field, nameof(authRequest.Field));

            var securityGroups = authRequest.Field.SecurityGroups;
            if (securityGroups == null || !securityGroups.Any())
                return FieldSecurityChallengeResult.Skipped(claimsUser);

            foreach (var group in securityGroups)
            {
                if (group.AllowAnonymous)
                    continue;

                // dont inspect these services before this
                // point in case the only security requirements set are all "allow anonymous"
                // in which case auth services don't matter
                if (claimsUser?.Identity == null)
                {
                    return FieldSecurityChallengeResult.Unauthorized("The request contains no user context to validate.");
                }

                if (!claimsUser.Identity.IsAuthenticated)
                {
                    return FieldSecurityChallengeResult.Unauthorized($"The supplied {nameof(ClaimsPrincipal)} was not successfully authenticated.");
                }

                foreach (var rule in group)
                {
                    if (rule.IsNamedPolicy)
                    {
                        if (_authService == null)
                        {
                            return FieldSecurityChallengeResult.Fail(
                                "The field defines authorization policies but " +
                                $"no '{nameof(IAuthorizationService)}' exists to process them.");
                        }

                        // policy check via the authorization service
                        var authResult = await _authService.AuthorizeAsync(claimsUser, rule.PolicyName).ConfigureAwait(false);
                        if (!authResult.Succeeded)
                            return FieldSecurityChallengeResult.Unauthorized($"Access denied via policy '{rule.PolicyName}'.");
                    }

                    if (rule.AllowedRoles.Count > 0)
                    {
                        // check any defined roles
                        if (rule.AllowedRoles.All(x => !claimsUser.IsInRole(x)))
                            return FieldSecurityChallengeResult.Unauthorized("Access denied due to missing a required role.");
                    }

                    // all checks passed
                }
            }

            return FieldSecurityChallengeResult.Success(claimsUser);
        }
    }
}