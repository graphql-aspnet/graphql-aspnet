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
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A piece of middleware, on the authorization pipeline, that can successfuly authenticate a
    /// <see cref="IUserSecurityContext"/>.
    /// </summary>
    public class FieldAuthenticationMiddleware : IGraphFieldSecurityMiddleware
    {
        /// <inheritdoc />
        public async Task InvokeAsync(GraphFieldSecurityContext context, GraphMiddlewareInvocationDelegate<GraphFieldSecurityContext> next, CancellationToken cancelToken = default)
        {
            context.Logger?.FieldAuthenticationChallenge(context);

            // only attempt an authentication
            // if no result is already deteremined and if no user has already been authenticated
            IAuthenticationResult authenticationResult = null;
            if (context.Result == null && context.AuthenticatedUser == null)
            {
                ClaimsPrincipal user;
                FieldSecurityChallengeResult challengeResult;

                (user, authenticationResult, challengeResult) = await this.AuthenticateUser(context, cancelToken);
                context.AuthenticatedUser = user;
                context.Result = challengeResult;
            }

            context.Logger?.FieldAuthenticationChallengeResult(context, authenticationResult);

            await next.Invoke(context, cancelToken).ConfigureAwait(false);
        }

        private async Task<(ClaimsPrincipal, IAuthenticationResult, FieldSecurityChallengeResult)> AuthenticateUser(GraphFieldSecurityContext context, CancellationToken cancelToken)
        {
            // Step 0: No authorization needed? just use the default user on the security context
            if (context.Field.SecurityGroups == null || !context.Field.SecurityGroups.Any())
                return (context.SecurityContext?.DefaultUser, null, null);

            // Step 1: For all the stacked security group in the checked field is there a path
            //         through the chain for any given authentication scheme
            bool defaultAuthSchemeIsAllowed = true;
            var allowedSchemes = new HashSet<string>();
            foreach (var group in context.Field.SecurityGroups)
            {
                if (group.AllowAnonymous)
                    continue;

                foreach (var rule in group)
                {
                    if (rule.AuthenticationSchemes.Count == 0)
                        continue;

                    defaultAuthSchemeIsAllowed = false;
                    if (allowedSchemes.Count == 0)
                    {
                        // when required schemes are encounted for the first time
                        // set them up as being required for the whole field
                        foreach (var scheme in rule.AuthenticationSchemes)
                            allowedSchemes.Add(scheme);
                    }
                    else
                    {
                        // when required schemes are encounted a second time
                        // ensure that at least one of the encounted schemes
                        // is already in list, if its not then the path to authenticate
                        // this field is bogus and can never be achieved.
                        var matchedSchemes = new HashSet<string>();
                        foreach (var scheme in rule.AuthenticationSchemes)
                        {
                            if (allowedSchemes.Contains(scheme))
                                matchedSchemes.Add(scheme);
                        }

                        if (matchedSchemes.Count == 0)
                        {
                            // when no match found fail out.
                            var failure = FieldSecurityChallengeResult.Fail(
                                $"The field '{context.Field.Route}' has mismatched required authentication schemes in its security groups. It contains " +
                                $"no scenarios where an authentication scheme can be used to authenticate a user to all possible security groups.");

                            return (null, null, failure);
                        }
                        else
                        {
                            // reduce the "allowed schemes" to just those that also match this
                            // security group
                            allowedSchemes = matchedSchemes;
                        }
                    }
                }
            }

            if (context.SecurityContext == null)
            {
                var failure = FieldSecurityChallengeResult.Fail(
                           $"No user could be authenticated because no security context was available on the request.");

                return (null, null, failure);
            }

            // Step 2: Attempt to authenticate the user against hte acceptable schemes
            IAuthenticationResult authTicket = null;
            if (defaultAuthSchemeIsAllowed)
            {
                // try the default scheme first
                authTicket = await context.SecurityContext.Authenticate(cancelToken);
                if (authTicket != null && !authTicket.Suceeded)
                    authTicket = null;
            }

            if (authTicket == null)
            {
                // try the other schemes
                foreach (var scheme in allowedSchemes)
                {
                    authTicket = await context.SecurityContext.Authenticate(scheme, cancelToken);
                    if (authTicket?.Suceeded ?? false)
                        break;

                    authTicket = null;
                }
            }

            if (authTicket == null || !authTicket.Suceeded)
            {
                var failure = FieldSecurityChallengeResult.UnAuthenticated(
                           $"No user could be authenticated using the approved authentication schemes.");

                return (null, authTicket, failure);
            }

            return (authTicket.User, authTicket, null);
        }
    }
}