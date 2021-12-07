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
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework.ServerBuilders;
    using Moq;

    /// <summary>
    /// A security context that can be used on a graph query execution that returns
    /// an exact claims principal per allowed auth scheme type.
    /// </summary>
    public class TestUserSecurityContext : IUserSecurityContext
    {
        private const string DEFAULT_SCHEME = "graphql.testing.defaultscheme";

        private Dictionary<string, ClaimsPrincipal> _userByScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserSecurityContext"/> class.
        /// </summary>
        public TestUserSecurityContext()
        {
            _userByScheme = new Dictionary<string, ClaimsPrincipal>();
        }

        /// <summary>
        /// Adds a fake user to this context mimicing hte claims principal that would be
        /// generated as a result of authenticating with the given <paramref name="authScheme"/>.
        /// </summary>
        /// <param name="authScheme">The authentication scheme.</param>
        /// <param name="isAuthenticated">if set to <c>true</c> the created user
        /// will be considered "authenticated".</param>
        /// <param name="claims">The claims to add the the principal.</param>
        /// <param name="roles">The roles to add to the principal.</param>
        public void AddFakeUser(string authScheme, bool isAuthenticated, IEnumerable<Claim> claims, IEnumerable<string> roles)
        {
            var schemeKey = authScheme ?? DEFAULT_SCHEME;
            claims = claims ?? Enumerable.Empty<Claim>();
            roles = roles ?? Enumerable.Empty<string>();

            var claimsPrincipal = new ClaimsPrincipal();
            var claimsToAdd = new List<Claim>();
            claimsToAdd.AddRange(claims);
            foreach (var role in roles)
                claimsToAdd.Add(new Claim(TestAuthorizationBuilder.ROLE_CLAIM_TYPE, role));

            ClaimsIdentity identity;
            if (claimsToAdd.Any() || isAuthenticated)
            {
                identity = new ClaimsIdentity(
                    claimsToAdd,
                    TestAuthorizationBuilder.AUTH_SCHEMA,
                    TestAuthorizationBuilder.USERNAME_CLAIM_TYPE,
                    TestAuthorizationBuilder.ROLE_CLAIM_TYPE);
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            claimsPrincipal.AddIdentity(identity);
            _userByScheme.Add(schemeKey, claimsPrincipal);
            if (authScheme == DEFAULT_SCHEME || this.DefaultUser == null)
                this.DefaultUser = claimsPrincipal;
        }

        /// <inheritdoc />
        public Task<IAuthenticationResult> Authenticate(string scheme, CancellationToken token = default)
        {
            var schemeKey = scheme ?? DEFAULT_SCHEME;

            ClaimsPrincipal user = null;
            if (_userByScheme.ContainsKey(schemeKey))
                user = _userByScheme[schemeKey];

            var mock = new Mock<IAuthenticationResult>();
            mock.Setup(x => x.AuthenticationScheme).Returns(scheme);
            mock.Setup(x => x.User).Returns(user);
            mock.Setup(x => x.Suceeded).Returns(user != null);

            return Task.FromResult(mock.Object);
        }

        /// <inheritdoc />
        public Task<IAuthenticationResult> Authenticate(CancellationToken token = default)
        {
            return this.Authenticate(null, token);
        }

        /// <inheritdoc />
        public ClaimsPrincipal DefaultUser { get; private set; }
    }
}