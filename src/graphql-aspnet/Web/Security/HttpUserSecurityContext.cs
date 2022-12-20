// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web.Security
{
    using System;
    using System.Collections.Generic;
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
        // Authors Note:
        //
        // Kevin Carroll (11/2022)
        //
        // This class was originally used a ConcurrentDictionary (instead of dictionary)
        // However, after a performance analysis, the speed at which this
        // object is created (1 per request), concurrent dictionary performs too many
        // internal allocations while containing 0 (or may be 1) objects
        // in many cases this was causing an enormous amount of GC
        // pressure(at 5k requests/sec).
        //
        // It was decided to push this object back to a standard
        // dictionary with a locking mechanism.
        //
        // Given scope its unlikely object will be under heavy pressure and
        // need the speed of lock free reads provided by ConcurrentDictionary
        // ------
        private const string DEFAULT_SCHEME = "-GraphQL.ASPNET.DefaultSchemeKey-";
        private readonly SemaphoreSlim _slim = new SemaphoreSlim(1);
        private readonly HttpContext _httpContext;
        private readonly Dictionary<string, IAuthenticationResult> _authResults;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUserSecurityContext"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context that governs this user context.</param>
        public HttpUserSecurityContext(HttpContext httpContext)
        {
            _httpContext = Validation.ThrowIfNullOrReturn(httpContext, nameof(httpContext));
            _authResults = new Dictionary<string, IAuthenticationResult>();
        }

        /// <inheritdoc />
        public Task<IAuthenticationResult> AuthenticateAsync(CancellationToken token = default)
        {
            return this.AuthenticateAsync(null, token);
        }

        /// <inheritdoc />
        public async Task<IAuthenticationResult> AuthenticateAsync(string scheme, CancellationToken token = default)
        {
            var schemeKey = scheme ?? DEFAULT_SCHEME;

            IAuthenticationResult authResult;
            lock (_authResults)
            {
                if (_authResults.TryGetValue(schemeKey, out authResult))
                    return authResult;
            }

            // multiple field resolution pipelines could be authenticating at
            // the same time for the same request.
            // and we can't lock over an async method, so we use a
            // semaphore when an authentication action needs to happen
            await _slim.WaitAsync(token).ConfigureAwait(false);

            try
            {
                lock (_authResults)
                {
                    if (_authResults.TryGetValue(schemeKey, out authResult))
                        return authResult;
                }

                // this can throw a "scheme not registered" exception
                // allow it to bubble out and fail don't trap it as an "unauthenticated"
                var result = await _httpContext.AuthenticateAsync(scheme);
                authResult = new HttpContextAuthenticationResult(scheme, result);
                lock (_authResults)
                {
                    if (!_authResults.ContainsKey(schemeKey))
                        _authResults.Add(schemeKey, authResult);
                }
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