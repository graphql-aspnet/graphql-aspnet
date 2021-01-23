// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldAuthorization.Components
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A piece of middleware, on the authorization pipeline, that can successfuly authorize a single user
    /// to a single field of data.
    /// </summary>
    internal class FieldAuthorizationCheckMiddleware : IGraphFieldAuthorizationMiddleware
    {
        private readonly IAuthorizationService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthorizationCheckMiddleware" /> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public FieldAuthorizationCheckMiddleware(IAuthorizationService authService = null)
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
        public async Task InvokeAsync(GraphFieldAuthorizationContext context, GraphMiddlewareInvocationDelegate<GraphFieldAuthorizationContext> next, CancellationToken cancelToken)
        {
            context.Logger?.FieldResolutionSecurityChallenge(context);

            var result = await this.AuthorizeRequest(context.Request, context.User).ConfigureAwait(false);
            context.Result = result ?? FieldAuthorizationResult.Default();

            context.Logger?.FieldResolutionSecurityChallengeResult(context);

            await next(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to authorize the request using the field based <see cref="IAuthorizeData" /> attributes.
        /// </summary>
        /// <param name="authRequest">The request to authorize.</param>
        /// <param name="claimsUser">The claims user provisioned for this run.</param>
        /// <returns>FieldAuthorizationResult.</returns>
        private async Task<FieldAuthorizationResult> AuthorizeRequest(IGraphFieldAuthorizationRequest authRequest, ClaimsPrincipal claimsUser)
        {
            Validation.ThrowIfNull(authRequest?.Field, nameof(authRequest.Field));

            var securityGroups = authRequest.Field.SecurityGroups;
            if (securityGroups == null || !securityGroups.Any())
            {
                return FieldAuthorizationResult.Skipped();
            }

            // each security group represents one level of security requirements (e.g. the controller group then the action)
            // the user must autenticate to each of them in turn.
            foreach (var group in securityGroups)
            {
                if (group.AllowAnonymous)
                    continue;

                // dont inspect these services before this
                // point in case the only security requirements set are all "allow anonymous"
                // in which case auth services don't matter
                if (claimsUser?.Identity == null)
                {
                    return FieldAuthorizationResult.Fail("The request contains no user context to validate.");
                }

                if (!claimsUser.Identity.IsAuthenticated)
                {
                    return FieldAuthorizationResult.Fail($"The supplied {nameof(ClaimsPrincipal)} was not successfully authenticated.");
                }

                foreach (var rule in group)
                {
                    if (rule.IsNamedPolicy)
                    {
                        if (_authService == null)
                        {
                            return FieldAuthorizationResult.Fail(
                                "The field defines authorization policies but " +
                                $"no '{nameof(IAuthorizationService)}' exists to process them.");
                        }

                        // policy check via the authorization service
                        var authResult = await _authService.AuthorizeAsync(claimsUser, rule.PolicyName).ConfigureAwait(false);
                        if (!authResult.Succeeded)
                            return FieldAuthorizationResult.Fail($"Access denied via policy '{rule.PolicyName}'.");
                    }

                    if (rule.AllowedRoles.Count > 0)
                    {
                        // check any defined roles
                        if (rule.AllowedRoles.All(x => !claimsUser.IsInRole(x)))
                            return FieldAuthorizationResult.Fail("Access denied due to missing a required role.");
                    }

                    if (rule.AuthenticationSchemes.Count > 0)
                    {
                        // check against any limiting authentication schemes
                        if (claimsUser.Identities.All(x => !rule.AuthenticationSchemes.Contains(x.AuthenticationType)))
                            return FieldAuthorizationResult.Fail("Access denied due to missing a required authentication scheme.");
                    }

                    // all checks passed
                }
            }

            return FieldAuthorizationResult.Success();
        }
    }
}