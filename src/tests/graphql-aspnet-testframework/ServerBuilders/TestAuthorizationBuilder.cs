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

        private bool _includeAuthProvider;
        private AuthorizationPolicyBuilder _standardDefaultPolicyBuilder;
        private AuthorizationPolicyBuilder _defaultPolicyBuilder;
        private List<KeyValuePair<string, AuthorizationPolicyBuilder>> _policyBuilders;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthorizationBuilder" /> class.
        /// </summary>
        /// <param name="enableAuthServices">if set to <c>true</c> authorization services will be configured for this instance.</param>
        public TestAuthorizationBuilder(bool enableAuthServices = true)
        {
            _policyBuilders = new List<KeyValuePair<string, AuthorizationPolicyBuilder>>();
            _defaultPolicyBuilder = null;
            _includeAuthProvider = enableAuthServices;

            _standardDefaultPolicyBuilder = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser();
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
                    foreach (var kvp in _policyBuilders)
                        o.AddPolicy(kvp.Key, kvp.Value.Build());

                    var defaultPolicyToUse = _defaultPolicyBuilder ?? _standardDefaultPolicyBuilder;
                    if (defaultPolicyToUse != null)
                        o.DefaultPolicy = defaultPolicyToUse.Build();
                });
            }
        }

        /// <summary>
        /// Disables the authorization provider, no provider and no policies
        /// will not be injected into the server.
        /// </summary>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthorizationBuilder DisableAuthorization()
        {
            _includeAuthProvider = false;
            return this;
        }

        /// <summary>
        /// Adds a simple new policy to the service based on a list of roles. When enforcing this policy
        /// the authorization service will require an authenticated user and
        /// check to see if the user belongs to any role in the list.
        /// </summary>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="roleList">A comma-sepeated list of roles.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthorizationBuilder AddRolePolicy(string policyName, string roleList)
        {
            this.NewPolicy(policyName)
                .RequireRole(roleList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                .RequireAuthenticatedUser();

            return this;
        }

        /// <summary>
        /// Adds a simple new policy to the service requiring that the user be authenticated
        /// and have the given claim type and value pair.
        /// </summary>
        /// <param name="policyName">The name of the policy.</param>
        /// <param name="claimType">The claim type to require.</param>
        /// <param name="claimValue">The claim value to require.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthorizationBuilder AddClaimPolicy(string policyName, string claimType, string claimValue)
        {
            this.NewPolicy(policyName)
                .RequireClaim(claimType, claimValue)
                .RequireAuthenticatedUser();

            return this;
        }

        /// <summary>
        /// Adds a default policy to the <see cref="IAuthorizationService" /> service, mimicing an action similar to calling
        /// <c>AddAuthorization</c> and setting a <c>DefaultPolicy</c> during startup.
        /// </summary>
        /// <returns>TestAuthorizationBuilder.</returns>
        public AuthorizationPolicyBuilder DefaultPolicy()
        {
            _defaultPolicyBuilder = _defaultPolicyBuilder ?? new AuthorizationPolicyBuilder();
            return _defaultPolicyBuilder;
        }

        /// <summary>
        /// Creates a new policy via a raw builder allowing for the testing of an open ended
        /// policy.
        /// </summary>
        /// <param name="policyName">The unique name to give to this policy.</param>
        /// <returns>AuthorizationPolicyBuilder.</returns>
        public AuthorizationPolicyBuilder NewPolicy(string policyName)
        {
            var kvp = new KeyValuePair<string, AuthorizationPolicyBuilder>(policyName, new AuthorizationPolicyBuilder());
            _policyBuilders.Add(kvp);
            return kvp.Value;
        }
    }
}