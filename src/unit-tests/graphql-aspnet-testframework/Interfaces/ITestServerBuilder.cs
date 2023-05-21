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
    /// A base interface describing parameters common to all schema specific
    /// test server builders.
    /// </summary>
    public interface ITestServerBuilder : IServiceCollection
    {
        /// <summary>
        /// Gets the authentication builder used to configure authentication
        /// parameters known the to test server.
        /// </summary>
        /// <value>TestAuthenticationBuilder.</value>
        public TestAuthenticationBuilder Authentication { get; }

        /// <summary>
        /// Gets the authorization builder used to configure the roles and policys known the to test server.
        /// </summary>
        /// <value>TestAuthorizationBuilder.</value>
        public TestAuthorizationBuilder Authorization { get; }

        /// <summary>
        /// Gets the builder to configure the creation of a mocked <see cref="IUserSecurityContext"/>.
        /// </summary>
        /// <value>TestUserSecurityContextBuilder.</value>
        public TestUserSecurityContextBuilder UserContext { get; }

        /// <summary>
        /// Gets the builder to configure the setup of the logging framework.
        /// </summary>
        /// <value>TestLoggingBuilder.</value>
        public TestLoggingBuilder Logging { get; }
    }
}