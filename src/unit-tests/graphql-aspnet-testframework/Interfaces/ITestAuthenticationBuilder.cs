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
    /// <summary>
    /// A test server component that can configure authentication scheme related
    /// parameters for the new server.
    /// </summary>
    public interface ITestAuthenticationBuilder : IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// Sets the default authentication scheme to use when authenticating accounts.
        /// This method is synonymous with supplying a default scheme
        /// when registering authentication via the <c>AddAuthentication(schemeName)</c>
        /// at startup.
        /// </summary>
        /// <param name="scheme">The scheme to simulate as the 'default configured scheme"
        /// for the server instance.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        ITestAuthenticationBuilder SetDefaultAuthenticationScheme(string scheme);

        /// <summary>
        /// Gets the configured default authentication scheme
        /// to use when authenticating users.
        /// </summary>
        /// <value>The default authentication scheme.</value>
        string DefaultAuthenticationScheme { get; }
    }
}