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
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder for generating a <see cref="ClaimsPrincipal" /> from a set of claims and roles.
    /// </summary>
    public class TestUserSecurityContextBuilder : IGraphTestFrameworkComponent
    {
        /// <summary>
        /// The scheme under which the user is authenticated if no scheme
        /// is supplied.
        /// </summary>
        public const string DEFAULT_SCHEME = TestAuthenticationBuilder.DEFAULT_AUTH_SCHEMA;

        private readonly List<string> _userRoles;
        private readonly List<Claim> _userClaims;
        private readonly ITestServerBuilder _parentServerBuilder;
        private string _authScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserSecurityContextBuilder"/> class.
        /// </summary>
        /// <param name="serverBuilder">The server builder.</param>
        public TestUserSecurityContextBuilder(ITestServerBuilder serverBuilder)
        {
            _authScheme = null;
            _parentServerBuilder = serverBuilder;
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
        /// Authenticates the user with the default username, on the "default scheme", but assigns no roles or other claims.
        /// </summary>
        /// <param name="username">The username to assign to the user.</param>
        /// <param name="authScheme">The authentication scheme under which the user should be authenticated.</param>
        /// <param name="usernameClaimType">The name of the claim that will hold the username on the <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>TestUserAccountBuilder.</returns>
        public TestUserSecurityContextBuilder Authenticate(string username = "john-doe", string authScheme = DEFAULT_SCHEME, string usernameClaimType = TestAuthorizationBuilder.USERNAME_CLAIM_TYPE)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username));

            if (string.IsNullOrWhiteSpace(authScheme))
                throw new ArgumentException(nameof(authScheme));

            if (string.IsNullOrWhiteSpace(usernameClaimType))
                throw new ArgumentException(nameof(usernameClaimType));

            _authScheme = authScheme;
            return this.AddUserClaim(usernameClaimType, username);
        }

        /// <summary>
        /// Adds an authorized claim of the given type and value to the user account. The user is automatically authenticated
        /// when the claim is added.
        /// </summary>
        /// <param name="claimType">The claim type.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserSecurityContextBuilder AddUserClaim(string claimType, string claimValue)
        {
            _userClaims.Add(new Claim(claimType, claimValue));
            return this;
        }

        /// <summary>
        /// Adds a single, authorized role to the user account. The user is automatically authenticated
        /// when the role is added.
        /// </summary>
        /// <param name="roleName">Name of the role to add.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestUserSecurityContextBuilder AddUserRole(string roleName)
        {
            _userRoles.Add(roleName);
            return this;
        }

        /// <summary>
        /// Creates a new user account with the user settings defined in this builder.
        /// </summary>
        /// <returns>ClaimsPrincipal.</returns>
        public IUserSecurityContext CreateSecurityContext()
        {
            var context = new TestUserSecurityContext(_parentServerBuilder?.Authentication?.DefaultAuthScheme);
            context.Setup(_authScheme, _userClaims, _userRoles);
            return context;
        }
    }
}