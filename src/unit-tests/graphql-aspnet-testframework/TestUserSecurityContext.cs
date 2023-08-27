// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Security;
    using NSubstitute;

    /// <summary>
    /// A security context that can be used on a graph query execution that returns
    /// an exact claims principal per allowed auth scheme type.
    /// </summary>
    public class TestUserSecurityContext : IUserSecurityContext
    {
        private readonly string _defaultAuthScheme;
        private string _schemeAuthedWith;
        private ClaimsPrincipal _userPrincipal = null;
        private ClaimsPrincipal _defaultUser = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserSecurityContext" /> class.
        /// </summary>
        /// <param name="defaultAuthScheme">The default authentication scheme
        /// to use, alowing this test context to mimic the fallback
        /// chain used when no scheme is provided on a
        /// <c>IAuthorizeData</c> item but one supplied during <c>AddAuthentication</c> setup.</param>
        public TestUserSecurityContext(string defaultAuthScheme)
        {
            _defaultAuthScheme = defaultAuthScheme;
        }

        /// <summary>
        /// Assembles the fake user on this context mimicing the claims principal that would be
        /// generated as a result of authenticating with the given <paramref name="authSchemeUsed"/>.
        /// </summary>
        /// <param name="authSchemeUsed">The authentication scheme the user
        /// is authenticated under. If null, the user will be set as "not authenticated".</param>
        /// <param name="claims">The claims to add the the principal.</param>
        /// <param name="roles">The roles to add to the principal.</param>
        public virtual void Setup(string authSchemeUsed, IEnumerable<Claim> claims, IEnumerable<string> roles)
        {
            _schemeAuthedWith = authSchemeUsed;
            var isAuthenticated = authSchemeUsed != null;
            if (isAuthenticated)
            {
                claims = claims ?? Enumerable.Empty<Claim>();
                roles = roles ?? Enumerable.Empty<string>();

                var claimsPrincipal = new ClaimsPrincipal();
                var claimsToAdd = new List<Claim>();
                claimsToAdd.AddRange(claims);
                foreach (var role in roles)
                    claimsToAdd.Add(new Claim(TestFrameworkConstants.ROLE_CLAIM_TYPE, role));

                ClaimsIdentity identity;
                if (isAuthenticated)
                {
                    identity = new ClaimsIdentity(
                        claimsToAdd,
                        TestFrameworkConstants.DEFAULT_AUTH_SCHEME,
                        TestFrameworkConstants.USERNAME_CLAIM_TYPE,
                        TestFrameworkConstants.ROLE_CLAIM_TYPE);
                }
                else
                {
                    identity = new ClaimsIdentity();
                }

                claimsPrincipal.AddIdentity(identity);
                _userPrincipal = claimsPrincipal;
                _defaultUser = _userPrincipal;
            }
        }

        /// <inheritdoc />
        public virtual Task<IAuthenticationResult> AuthenticateAsync(string scheme, CancellationToken token = default)
        {
            var schemeToCheckAgainst = scheme ?? _defaultAuthScheme;

            ClaimsPrincipal user = null;
            if (_schemeAuthedWith != null && _schemeAuthedWith == schemeToCheckAgainst)
                user = _userPrincipal;

            var mock = Substitute.For<IAuthenticationResult>();
            mock.AuthenticationScheme.Returns(scheme);
            mock.User.Returns(user);
            mock.Suceeded.Returns(user != null);

            return Task.FromResult(mock);
        }

        /// <inheritdoc />
        public virtual Task<IAuthenticationResult> AuthenticateAsync(CancellationToken token = default)
        {
            return this.AuthenticateAsync(null, token);
        }

        /// <inheritdoc />
        public ClaimsPrincipal DefaultUser => _defaultUser;
    }
}