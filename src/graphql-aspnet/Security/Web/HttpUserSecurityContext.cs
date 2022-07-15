// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Security.Web
{
    using System;
    using System.Collections.Concurrent;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Security;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A security context that uses an underlying <see cref="HttpContext"/> to authenticate
    /// and authorize a user.
    /// </summary>
    public class HttpUserSecurityContext : IUserSecurityContext, IDisposable
    {
        private const string DEFAULT_SCHEME = "-GraphQL.ASPNET.DefaultSchemeKey-";
        private readonly SemaphoreSlim _slim = new SemaphoreSlim(1);
        private readonly HttpContext _httpContext;
        private readonly ConcurrentDictionary<string, IAuthenticationResult> _authResults;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUserSecurityContext"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context that governs this user context.</param>
        public HttpUserSecurityContext(HttpContext httpContext)
        {
            _httpContext = Validation.ThrowIfNullOrReturn(httpContext, nameof(httpContext));
            _authResults = new ConcurrentDictionary<string, IAuthenticationResult>();
        }

        /// <inheritdoc />
        public Task<IAuthenticationResult> Authenticate(CancellationToken token = default)
        {
            return this.Authenticate(null, token);
        }

        /// <inheritdoc />
        public async Task<IAuthenticationResult> Authenticate(string scheme, CancellationToken token = default)
        {
            var schemeKey = scheme ?? DEFAULT_SCHEME;

            if (_authResults.TryGetValue(schemeKey, out var authResult))
                return authResult;

            // multiple field resolution pipelines could be authenticating at
            // the same time for the same request.
            await _slim.WaitAsync(token).ConfigureAwait(false);

            try
            {
                if (_authResults.TryGetValue(schemeKey, out authResult))
                    return authResult;

                // this can throw a "scheme not registered" exception
                // allow it to bubble out and fail don't trap it as an "unauthenticated"
                var result = await _httpContext.AuthenticateAsync(scheme);
                authResult = new HttpContextAuthenticationResult(scheme, result);
                _authResults.TryAdd(schemeKey, authResult);
            }
            finally
            {
                _slim.Release();
            }

            return authResult;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                    _slim.Dispose();

                _disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <para>
        /// Gets the claims principal representing the user as supplied by default for the underlying
        /// provider. This may or may not represent the claims principal used for a specific
        /// authentication scheme.
        /// </para>
        /// <remarks>
        /// For an <see cref="HttpContext"/> based security context this value is equivilant to
        /// <c>HttpContext.User</c>.
        /// </remarks>
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal DefaultUser => _httpContext?.User;
    }
}