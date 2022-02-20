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
    public class FieldAuthorizationMiddleware : IGraphFieldSecurityMiddleware
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
        public async Task InvokeAsync(GraphFieldSecurityContext context, GraphMiddlewareInvocationDelegate<GraphFieldSecurityContext> next, CancellationToken cancelToken = default)
        {
            context.Logger?.FieldAuthorizationChallenge(context);

            // a result may have been set by other middleware
            // in this auth pipeline, if a result is already determined just skip this step
            if (context.Result == null)
            {
                var result = await this.AuthorizeRequest(context).ConfigureAwait(false);
                context.Result = result ?? FieldSecurityChallengeResult.Default();
            }

            context.Logger?.FieldAuthorizationChallengeResult(context);

            await next(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to authorize the request using the field based <see cref="IAuthorizeData" /> attributes.
        /// </summary>
        /// <param name="context">The context to process.</param>
        /// <returns>FieldSecurityChallengeResult.</returns>
        private async Task<FieldSecurityChallengeResult> AuthorizeRequest(GraphFieldSecurityContext context)
        {
            Validation.ThrowIfNull(context?.SecurityRequirements, nameof(GraphFieldSecurityContext.SecurityRequirements));

            var claimsUser = context.AuthenticatedUser;

            // when there is nothing to enforce, just skip
            var anyPoliciesToEnforce = context.SecurityRequirements.EnforcedPolicies.Any();
            var anyRolesToCheck = context.SecurityRequirements.EnforcedRoleGroups.Any();
            if (!anyPoliciesToEnforce
                && !anyRolesToCheck)
            {
                return FieldSecurityChallengeResult.Skipped(claimsUser);
            }

            if (context.SecurityRequirements.AllowAnonymous)
                return FieldSecurityChallengeResult.Success(claimsUser);

            if (anyPoliciesToEnforce && _authService == null)
            {
                return FieldSecurityChallengeResult.Fail(
                    "The field defines authorization policies but " +
                    $"no '{nameof(IAuthorizationService)}' exists to enforce them.");
            }

            // ensure all policies are met
            foreach (var policy in context.SecurityRequirements.EnforcedPolicies)
            {
                var authResult = await _authService.AuthorizeAsync(claimsUser, policy.Policy).ConfigureAwait(false);
                if (!authResult.Succeeded)
                    return FieldSecurityChallengeResult.Unauthorized($"Access denied via policy '{policy.Name}'.");
            }

            // for any roles groups defined in the hierarchy of [Authorize]
            // statements ensure the user belongs to at least one from each level
            foreach (var roleSet in context.SecurityRequirements.EnforcedRoleGroups)
            {
                var hasARole = roleSet.Any(roleName => (claimsUser?.IsInRole(roleName) ?? false));
                if (!hasARole)
                {
                    var roleNames = string.Join(", ", roleSet.Select(roleName => $"'{roleName}'"));
                    return FieldSecurityChallengeResult.Unauthorized($"Access denied. User must belong to at least one role: {roleNames}.");
                }
            }

            return FieldSecurityChallengeResult.Success(claimsUser);
        }
    }
}