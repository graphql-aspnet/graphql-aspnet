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
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// A piece of middleware, on the authorization pipeline, that can successfuly authenticate a
    /// <see cref="IUserSecurityContext"/>.
    /// </summary>
    public class FieldAuthenticationMiddleware : IGraphFieldSecurityMiddleware, IDisposable
    {
        private SemaphoreSlim _locker = new SemaphoreSlim(1);
        private IAuthenticationSchemeProvider _schemeProvider;
        private bool _defaultsSet;
        private string _defaultScheme;
        private ImmutableHashSet<string> _allKnownSchemes;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthenticationMiddleware" /> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider used to determine
        /// various authentication defaults.</param>
        public FieldAuthenticationMiddleware(IAuthenticationSchemeProvider schemeProvider = null)
        {
            _schemeProvider = schemeProvider;
            _allKnownSchemes = ImmutableHashSet.Create<string>();
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
                var failure = FieldSecurityChallengeResult.UnAuthenticated(
                    $"No user could be authenticated because the request indicated no user could ever be authorized.");

                return (null, null, failure);
            }

            var canBeAnonymous = context.SecurityRequirements.AllowAnonymous;

            await this.EnsureDefaults();

            // Step 2: Attempt to authenticate the user against the acceptable schemes
            IAuthenticationResult authTicket = null;

            // when no explicit schemes are defined
            // attempt to fall back to the default scheme configured on this
            // app instance (if one even exists)
            if (context.SecurityRequirements.AllowedAuthenticationSchemes.Count == 0)
            {
                authTicket = await this.AuthenticateUserToScheme(
                    context.SecurityContext,
                    _defaultScheme,
                    !canBeAnonymous,
                    cancelToken);
            }
            else
            {
                foreach (var scheme in context.SecurityRequirements.AllowedAuthenticationSchemes)
                {
                    authTicket = await this.AuthenticateUserToScheme(context.SecurityContext, scheme.AuthScheme, !canBeAnonymous, cancelToken);
                    if (authTicket?.Suceeded ?? false)
                        break;

                    authTicket = null;
                }
            }

            // if no auth is "required" then return back
            // but include the user that was found (when it was)
            if (context.SecurityRequirements.AllowAnonymous)
            {
                if (authTicket != null && authTicket.Suceeded)
                    return (authTicket.User, authTicket, null);
                else
                    return (context.SecurityContext?.DefaultUser, null, null);
            }

            // when anonymous is not allowed
            // then a user must be authenticated
            // when no user could have been authed fail with a
            // special message
            if (context.SecurityContext == null)
            {
                var failure = FieldSecurityChallengeResult.Fail(
                           $"No user could be authenticated because no security context was available on the request.");

                return (null, null, failure);
            }

            // when authentication failed
            // indicate as such
            if (authTicket == null || !authTicket.Suceeded)
            {
                var failure = FieldSecurityChallengeResult.UnAuthenticated(
                           $"No user could be authenticated using the approved authentication schemes.");

                return (null, authTicket, failure);
            }

            // authentication successful!
            return (authTicket.User, authTicket, null);
        }

        /// <summary>
        /// Extracts the default and all known schemes available to this instance.
        /// </summary>
        private async Task EnsureDefaults()
        {
            if (_defaultsSet)
                return;

            await _locker.WaitAsync();
            try
            {
                if (_defaultsSet)
                    return;

                _defaultsSet = true;
                if (_schemeProvider != null)
                {
                    var foundDefault = await _schemeProvider.GetDefaultAuthenticateSchemeAsync();
                    _defaultScheme = foundDefault?.Name;

                    var allSchemes = await _schemeProvider.GetAllSchemesAsync();
                    if (allSchemes != null)
                    {
                        _allKnownSchemes = allSchemes.Select(x => x.Name)
                            .ToImmutableHashSet();
                    }
                }
            }
            finally
            {
                _locker.Release();
            }
        }

        private async Task<IAuthenticationResult> AuthenticateUserToScheme(
            IUserSecurityContext userContext,
            string scheme,
            bool shouldThrowOnFail,
            CancellationToken cancelToken)
        {
            // since authenticate can result in an exception, espeically when no default exsists
            // or some scheme is defined that doesnt really exist. We want those exceptions to bubble
            // (and prevent execution) in cases where the method completing is needed (i.e. when auth is absolutely required)
            // but in cases where anonymous is allowed there is no reason to fail, we just let
            // execution continue without authentication
            if (!shouldThrowOnFail)
            {
                if (!_allKnownSchemes.Contains(scheme))
                    return null;
            }

            if (userContext != null)
                return await userContext.Authenticate(scheme, cancelToken);
            else
                return null;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _locker.Dispose();
                }

                _allKnownSchemes = null;
                disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}