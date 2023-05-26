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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Builds an authorization chain for a single user and policy setup.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this component is targeting.</typeparam>
    public class TestAuthorizationBuilder<TSchema> : IGraphQLTestFrameworkComponent<TSchema>, ITestAuthorizationBuilder
        where TSchema : class, ISchema
    {
        private bool _includeAuthProvider;
        private AuthorizationPolicyBuilder _standardDefaultPolicyBuilder;
        private AuthorizationPolicyBuilder _defaultPolicyBuilder;
        private List<KeyValuePair<string, AuthorizationPolicyBuilder>> _policyBuilders;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthorizationBuilder{TSchema}" /> class.
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

        /// <inheritdoc />
        public virtual void Inject(IServiceCollection serviceCollection)
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

        /// <inheritdoc />
        public virtual ITestAuthorizationBuilder DisableAuthorization()
        {
            _includeAuthProvider = false;
            return this;
        }

        /// <inheritdoc />
        public virtual ITestAuthorizationBuilder AddRolePolicy(string policyName, string roleList)
        {
            this.NewPolicy(policyName)
                .RequireRole(roleList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                .RequireAuthenticatedUser();

            return this;
        }

        /// <inheritdoc />
        public virtual ITestAuthorizationBuilder AddClaimPolicy(string policyName, string claimType, string claimValue)
        {
            this.NewPolicy(policyName)
                .RequireClaim(claimType, claimValue)
                .RequireAuthenticatedUser();

            return this;
        }

        /// <inheritdoc />
        public virtual AuthorizationPolicyBuilder DefaultPolicy()
        {
            _defaultPolicyBuilder = _defaultPolicyBuilder ?? new AuthorizationPolicyBuilder();
            return _defaultPolicyBuilder;
        }

        /// <inheritdoc />
        public virtual AuthorizationPolicyBuilder NewPolicy(string policyName)
        {
            var kvp = new KeyValuePair<string, AuthorizationPolicyBuilder>(policyName, new AuthorizationPolicyBuilder());
            _policyBuilders.Add(kvp);
            return kvp.Value;
        }
    }
}