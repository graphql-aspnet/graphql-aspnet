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
    using System.Linq;
    using System.Security.Claims;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder for generating a <see cref="ClaimsPrincipal"/> from a set of claims and roles.
    /// </summary>
    public class TestUserAccountBuilder : IGraphTestFrameworkComponent
    {
        private readonly List<string> _userRoles;
        private readonly List<Claim> _userClaims;
        private bool _authenticateUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserAccountBuilder"/> class.
        /// </summary>
        public TestUserAccountBuilder()
        {
            _userRoles = new List<string>();
            _userClaims = new List<Claim>();
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
        /// Authenticates the user with the default username but assigns no roles or other claims.
        /// </summary>
        /// <returns>TestUserAccountBuilder.</returns>
        public TestUserAccountBuilder Authenticate()
        {
            return this.SetUsername("john-doe");
        }

        /// <summary>
        /// Sets the username as a claim on the principal with the given claim type.
        /// If not supplied an internal, default test value for the claim type will be used. The user is automatically authenticated
        /// when the username is set.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="claimType">The name of the claim to use when identifying usernames.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserAccountBuilder SetUsername(string username, string claimType = null)
        {
            _authenticateUser = true;
            claimType = claimType ?? TestAuthorizationBuilder.USERNAME_CLAIM_TYPE;
            return this.AddUserClaim(claimType, username);
        }

        /// <summary>
        /// Adds an authorized claim of the given type and value to the user account. The user is automatically authenticated
        /// when the claim is added.
        /// </summary>
        /// <param name="claimType">The claim type.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserAccountBuilder AddUserClaim(string claimType, string claimValue)
        {
            _authenticateUser = true;
            _userClaims.Add(new Claim(claimType, claimValue));
            return this;
        }

        /// <summary>
        /// Adds a single, authorized role to the user account. The user is automatically authenticated
        /// when the role is added.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserAccountBuilder AddUserRole(string roleName)
        {
            _authenticateUser = true;
            _userRoles.Add(roleName);
            return this;
        }

        /// <summary>
        /// Creates a new user account with the user settings defined in this builder.
        /// </summary>
        /// <returns>ClaimsPrincipal.</returns>
        public ClaimsPrincipal CreateUserAccount()
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var claimsToAdd = new List<Claim>();
            claimsToAdd.AddRange(_userClaims);
            foreach (var role in _userRoles)
                claimsToAdd.Add(new Claim(TestAuthorizationBuilder.ROLE_CLAIM_TYPE, role));

            ClaimsIdentity identity = null;
            if (claimsToAdd.Any() || _authenticateUser)
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
            return claimsPrincipal;
        }
    }
}