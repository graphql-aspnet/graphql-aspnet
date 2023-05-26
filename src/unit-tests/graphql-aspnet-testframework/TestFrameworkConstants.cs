// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System.Security.Claims;

    /// <summary>
    /// A set of constants used by the GraphQL ASP.NET test framework.
    /// </summary>
    public class TestFrameworkConstants
    {
        /// <summary>
        /// The default name of the authentication scheme used to "authenticate" a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string DEFAULT_AUTH_SCHEME = "graphql.default.scheme";

        /// <summary>
        /// The default claim type identifying a username for a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string USERNAME_CLAIM_TYPE = "GraphQLTestServer.NameClaim";

        /// <summary>
        /// The default claim type for roles issued to a generated <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public const string ROLE_CLAIM_TYPE = "GraphQLTestServer.RoleClaim";
    }
}