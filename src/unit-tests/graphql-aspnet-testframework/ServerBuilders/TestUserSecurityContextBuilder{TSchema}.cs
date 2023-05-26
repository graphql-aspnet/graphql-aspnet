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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder for generating a <see cref="ClaimsPrincipal" /> from a set of claims and roles.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this component is targeting.</typeparam>
    public class TestUserSecurityContextBuilder<TSchema> : IGraphQLTestFrameworkComponent<TSchema>, ITestUserSecurityContextBuilder
        where TSchema : class, ISchema
    {
        private readonly List<string> _userRoles;
        private readonly List<Claim> _userClaims;
        private readonly ITestServerBuilder _parentServerBuilder;
        private string _authScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserSecurityContextBuilder{TSchema}"/> class.
        /// </summary>
        /// <param name="serverBuilder">The server builder.</param>
        public TestUserSecurityContextBuilder(ITestServerBuilder serverBuilder)
        {
            _authScheme = null;
            _parentServerBuilder = serverBuilder;
            _userRoles = new List<string>();
            _userClaims = new List<Claim>();
        }

        /// <inheritdoc />
        public void Inject(IServiceCollection serviceCollection)
        {
            // nothing to inject for the user account that is created
        }

        /// <inheritdoc />
        public ITestUserSecurityContextBuilder Authenticate(string username = "john-doe", string authScheme = TestFrameworkConstants.DEFAULT_AUTH_SCHEME, string usernameClaimType = TestFrameworkConstants.USERNAME_CLAIM_TYPE)
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

        /// <inheritdoc />
        public ITestUserSecurityContextBuilder AddUserClaim(string claimType, string claimValue)
        {
            _userClaims.Add(new Claim(claimType, claimValue));
            return this;
        }

        /// <inheritdoc />
        public ITestUserSecurityContextBuilder AddUserRole(string roleName)
        {
            _userRoles.Add(roleName);
            return this;
        }

        /// <inheritdoc />
        public IUserSecurityContext CreateSecurityContext()
        {
            var context = new TestUserSecurityContext(_parentServerBuilder?.Authentication?.DefaultAuthenticationScheme);
            context.Setup(_authScheme, _userClaims, _userRoles);
            return context;
        }
    }
}