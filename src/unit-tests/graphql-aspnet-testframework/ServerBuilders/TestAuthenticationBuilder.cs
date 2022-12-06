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
    public class TestAuthenticationBuilder : IGraphTestFrameworkComponent
    {
        /// <summary>
        /// The default name of the authentication schema used to "authenticate" a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string DEFAULT_AUTH_SCHEMA = "graphql.default.scheme";

        private string _defaultAuthScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthenticationBuilder" /> class.
        /// </summary>
        public TestAuthenticationBuilder()
        {
            _defaultAuthScheme = DEFAULT_AUTH_SCHEMA;
        }

        /// <summary>
        /// Injects the component configured by this builder with a service collection instance.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public void Inject(IServiceCollection serviceCollection)
        {
        }

        /// <summary>
        /// Sets the default authenticatio scheme to use when authenticating accounts.
        /// This method is synonymous with supplying a default scheme
        /// when registering authentication via the <c>AddAuthentication(schemeName)</c>
        /// at startup.
        /// </summary>
        /// <param name="scheme">The scheme to use.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public TestAuthenticationBuilder SetDefaultAuthScheme(string scheme)
        {
            _defaultAuthScheme = scheme;
            return this;
        }

        /// <summary>
        /// Gets the configured default authentication scheme
        /// to use when authenticating users.
        /// </summary>
        /// <value>The default authentication scheme.</value>
        public string DefaultAuthScheme => _defaultAuthScheme;
    }
}