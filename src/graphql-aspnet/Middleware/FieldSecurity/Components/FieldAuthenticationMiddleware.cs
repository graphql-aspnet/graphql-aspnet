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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A piece of middleware, on the authorization pipeline, that can successfuly authenticate a
    /// <see cref="IUserSecurityContext"/>.
    /// </summary>
    public class FieldAuthenticationMiddleware : IGraphFieldSecurityMiddleware
    {
        private const string DEFAULT_AUTH_SCHEME_KEY = "~graphql.aspnet.default~";

        private ConcurrentDictionary<IGraphField, ImmutableHashSet<string>> _allowedSchemesPerField;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthenticationMiddleware"/> class.
        /// </summary>
        public FieldAuthenticationMiddleware()
        {
            _allowedSchemesPerField = new ConcurrentDictionary<IGraphField, ImmutableHashSet<string>>();
        }

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

        private async Task<(ClaimsPrincipal, IAuthenticationResult, FieldSecurityChallengeResult)>
            AuthenticateUser(GraphFieldSecurityContext context, CancellationToken cancelToken)
        {
            // Step 1: Initial check for null requirements or allowed anonymous access
            if (context.SecurityRequirements == null)
            {
                var failure = FieldSecurityChallengeResult.Fail(
                    $"No user could be authenticated because no security requirements were available on the request.");

                return (null, null, failure);
            }

            if (context.SecurityRequirements == FieldSecurityRequirements.AutoDeny)
            {
                var failure = FieldSecurityChallengeResult.Fail(
                    $"No user could be authenticated because the request indicated no user can be authorized.");

                return (null, null, failure);
            }

            if (context.SecurityRequirements.AllowAnonymous)
            {
                return (context.SecurityContext?.DefaultUser, null, null);
            }

            if (context.SecurityContext == null)
            {
                var failure = FieldSecurityChallengeResult.Fail(
                           $"No user could be authenticated because no security context was available on the request.");

                return (null, null, failure);
            }

            // Step 2: Attempt to authenticate the user against hte acceptable schemes
            // try any explciit schemes first
            IAuthenticationResult authTicket = null;
            foreach (var scheme in context.SecurityRequirements.RequiredAuthenticationSchemes)
            {
                authTicket = await context.SecurityContext.Authenticate(scheme, cancelToken);
                if (authTicket?.Suceeded ?? false)
                    break;

                authTicket = null;
            }

            // fall back to the default scheme for the app domain
            // if allowed for this request
            if (authTicket == null && context.SecurityRequirements.AllowDefaultAuthenticationScheme)
                authTicket = await context.SecurityContext.Authenticate(cancelToken);

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