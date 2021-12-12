// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.ServerBuilders
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder for generating a <see cref="ClaimsPrincipal"/> from a set of claims and roles.
    /// </summary>
    public class TestUserSecurityContextBuilder : IGraphTestFrameworkComponent
    {
        private const string DEFAULT_SCHEME = "graphql.testing.defaultscheme";

        private readonly HashSet<string> _knownSchemes;
        private readonly Dictionary<string, List<string>> _userRoles;
        private readonly Dictionary<string, List<Claim>> _userClaims;
        private readonly Dictionary<string, bool> _userIsAuthenticated;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserSecurityContextBuilder"/> class.
        /// </summary>
        public TestUserSecurityContextBuilder()
        {
            _knownSchemes = new HashSet<string>();
            _userRoles = new Dictionary<string, List<string>>();
            _userClaims = new Dictionary<string, List<Claim>>();
            _userIsAuthenticated = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Registers the component configured by this builder with a service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public void Inject(IServiceCollection serviceCollection)
        {
            // nothing to inject for the user account that is created
        }

        /// <summary>
        /// Authenticates the user with the default username, on the "default scheme", but assigns no roles or other claims.
        /// </summary>
        /// <param name="authScheme">The authentication scheme under which the user should be authenticated.</param>
        /// <returns>TestUserAccountBuilder.</returns>
        public TestUserSecurityContextBuilder Authenticate(string authScheme = null)
        {
            return this.SetUsername(
                "john-doe",
                authScheme: authScheme);
        }

        /// <summary>
        /// Sets the username as a claim on the principal with the given claim type.
        /// If not supplied an internal, default test value for the claim type will be used. The user is automatically authenticated
        /// when the username is set.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="claimType">The name of the claim to use when identifying usernames.</param>
        /// <param name="authScheme">The authentication scheme to apply this username to.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserSecurityContextBuilder SetUsername(string username, string claimType = null, string authScheme = null)
        {
            claimType = claimType ?? TestAuthorizationBuilder.USERNAME_CLAIM_TYPE;
            return this.AddUserClaim(claimType, username, authScheme);
        }

        /// <summary>
        /// Adds an authorized claim of the given type and value to the user account. The user is automatically authenticated
        /// when the claim is added.
        /// </summary>
        /// <param name="claimType">The claim type.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <param name="authScheme">The authentication scheme, under which this claim will be applied.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserSecurityContextBuilder AddUserClaim(string claimType, string claimValue, string authScheme = null)
        {
            authScheme = authScheme ?? DEFAULT_SCHEME;

            if (!_userIsAuthenticated.ContainsKey(authScheme))
                _userIsAuthenticated.Add(authScheme, false);

            if (!_userClaims.ContainsKey(authScheme))
                _userClaims.Add(authScheme, new List<Claim>());

            _knownSchemes.Add(authScheme);
            _userIsAuthenticated[authScheme] = true;
            _userClaims[authScheme].Add(new Claim(claimType, claimValue));
            return this;
        }

        /// <summary>
        /// Adds a single, authorized role to the user account. The user is automatically authenticated
        /// when the role is added.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="authScheme">The authentication scheme under which this role will be applied.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserSecurityContextBuilder AddUserRole(string roleName, string authScheme = null)
        {
            authScheme = authScheme ?? DEFAULT_SCHEME;

            if (!_userIsAuthenticated.ContainsKey(authScheme))
                _userIsAuthenticated.Add(authScheme, false);

            if (!_userRoles.ContainsKey(authScheme))
                _userRoles.Add(authScheme, new List<string>());

            _knownSchemes.Add(authScheme);
            _userIsAuthenticated[authScheme] = true;
            _userRoles[authScheme].Add(roleName);
            return this;
        }

        /// <summary>
        /// Creates a new user account with the user settings defined in this builder.
        /// </summary>
        /// <returns>ClaimsPrincipal.</returns>
        public IUserSecurityContext CreateSecurityContext()
        {
            var context = new TestUserSecurityContext();

            foreach (var scheme in _knownSchemes)
            {
                var actualScheme = scheme;
                if (actualScheme == DEFAULT_SCHEME)
                    actualScheme = null;

                IEnumerable<Claim> claims = null;
                IEnumerable<string> roles = null;
                var isAuthed = false;

                if (_userClaims.ContainsKey(scheme))
                    claims = _userClaims[scheme];
                if (_userRoles.ContainsKey(scheme))
                    roles = _userRoles[scheme];
                if (_userIsAuthenticated.ContainsKey(scheme))
                    isAuthed = _userIsAuthenticated[scheme];

                context.AddFakeUser(actualScheme, isAuthed, claims, roles);
            }

            return context;
        }
    }
}