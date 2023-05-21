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
    using System.Security.Claims;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Builds an authentication chain for a single user and policy setup.
    /// </summary>
    public class TestAuthenticationBuilder : IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// The default name of the authentication schema used to "authenticate" a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string DEFAULT_AUTH_SCHEMA = "graphql.default.scheme";

        private string _defaultAuthenticationScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthenticationBuilder" /> class.
        /// </summary>
        public TestAuthenticationBuilder()
        {
            _defaultAuthenticationScheme = DEFAULT_AUTH_SCHEMA;
        }

        /// <inheritdoc />
        public void Inject(IServiceCollection serviceCollection)
        {
            // nothing to inject for this builder
        }

        /// <summary>
        /// Sets the default authentication scheme to use when authenticating accounts.
        /// This method is synonymous with supplying a default scheme
        /// when registering authentication via the <c>AddAuthentication(schemeName)</c>
        /// at startup.
        /// </summary>
        /// <param name="scheme">The scheme to simulate as the 'default configured scheme"
        /// for the server instance.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthenticationBuilder SetDefaultAuthenticationScheme(string scheme)
        {
            _defaultAuthenticationScheme = scheme;
            return this;
        }

        /// <summary>
        /// Gets the configured default authentication scheme
        /// to use when authenticating users.
        /// </summary>
        /// <value>The default authentication scheme.</value>
        public string DefaultAuthenticationScheme => _defaultAuthenticationScheme;
    }
}