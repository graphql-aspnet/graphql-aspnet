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
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Builds an authorization chain for a single user and policy setup.
    /// </summary>
    public class TestAuthorizationBuilder : IGraphTestFrameworkComponent
    {
        /// <summary>
        /// The claim type identifying a username for a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string USERNAME_CLAIM_TYPE = "GraphQLTestServer.NameClaim";

        /// <summary>
        /// The claim type for roles issued to a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string ROLE_CLAIM_TYPE = "GraphQLTestServer.RoleClaim";

        /// <summary>
        /// The name of the authentication schema used to "authenticate" a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string AUTH_SCHEMA = "GraphQLTestServer.AuthSchema";

        private readonly List<Action<AuthorizationOptions>> _optionConfigurators;
        private bool _includeAuthProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthorizationBuilder" /> class.
        /// </summary>
        /// <param name="enableAuthServices">if set to <c>true</c> authorization services will be configured for this instance.</param>
        public TestAuthorizationBuilder(bool enableAuthServices = true)
        {
            _optionConfigurators = new List<Action<AuthorizationOptions>>();
            _includeAuthProvider = enableAuthServices;
        }

        /// <summary>
        /// Injects the component configured by this builder with a service collection instance.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public void Inject(IServiceCollection serviceCollection)
        {
            if (_includeAuthProvider)
            {
                serviceCollection.AddAuthorization((o) =>
                   {
                       foreach (var action in _optionConfigurators)
                           action(o);
                   });
            }
        }

        /// <summary>
        /// Disables the authorization provider, it will not be injected into the server.
        /// </summary>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthorizationBuilder DisableAuthorization()
        {
            _includeAuthProvider = false;
            return this;
        }

        /// <summary>
        /// Adds a new policy to the service based on a list of roles. When enforcing this policy
        /// the authorization service will check to see if the user belongs to any role in the list.
        /// </summary>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="roleList">A comma-sepeated list of roles.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthorizationBuilder AddRolePolicy(string policyName, string roleList)
        {
            _optionConfigurators.Add((o) =>
            {
                o.AddPolicy(policyName, (builder) =>
                {
                    builder.RequireRole(roleList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                });
            });

            return this;
        }

        /// <summary>
        /// Adds a new policy to the service requiring that the user have the given claim type and value pair.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="claimType">The claim type to require.</param>
        /// <param name="claimValue">The claim value to require.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthorizationBuilder AddClaimPolicy(string name, string claimType, string claimValue)
        {
            _optionConfigurators.Add((o) => o.AddPolicy(name, (builder) => builder.RequireClaim(claimType, claimValue)));
            return this;
        }
    }
}