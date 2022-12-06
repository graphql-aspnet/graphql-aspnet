// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Framework.Interfaces
{
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Tests.Framework.ServerBuilders;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder that can generate a test server for graphql.
    /// </summary>
    public interface ITestServerBuilder : IServiceCollection
    {
        /// <summary>
        /// Gets the authentication builder used to configure authentication
        /// parameters known the to test server.
        /// </summary>
        /// <value>The authorization.</value>
        public TestAuthenticationBuilder Authentication { get; }

        /// <summary>
        /// Gets the authorization builder used to configure the roles and policys known the to test server.
        /// </summary>
        /// <value>The authorization.</value>
        public TestAuthorizationBuilder Authorization { get; }

        /// <summary>
        /// Gets the builder to configure the creation of a mocked <see cref="IUserSecurityContext"/>.
        /// </summary>
        /// <value>The user.</value>
        public TestUserSecurityContextBuilder UserContext { get; }

        /// <summary>
        /// Gets the builder to configure the setup of the logging framework.
        /// </summary>
        /// <value>The logging.</value>
        public TestLoggingBuilder Logging { get; }
    }
}