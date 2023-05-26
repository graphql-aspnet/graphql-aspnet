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
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A context builder that can generate a user context for use during a
    /// with a graphql aspnet test server instance.
    /// </summary>
    public interface ITestUserSecurityContextBuilder : IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// Configures the user to be constructed in an authenticated state.
        /// </summary>
        /// <param name="username">The username to assign to the user.</param>
        /// <param name="authScheme">The authentication scheme under which the user should be authenticated.</param>
        /// <param name="usernameClaimType">The name of the claim that will hold the username on the <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>TestUserAccountBuilder.</returns>
        ITestUserSecurityContextBuilder Authenticate(string username = "john-doe", string authScheme = TestFrameworkConstants.DEFAULT_AUTH_SCHEME, string usernameClaimType = TestFrameworkConstants.USERNAME_CLAIM_TYPE);

        /// <summary>
        /// Adds an authorized claim of the given type and value to the user account.
        /// The user is automatically set to be authenticated when the claim is added.
        /// </summary>
        /// <param name="claimType">The claim type.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        ITestUserSecurityContextBuilder AddUserClaim(string claimType, string claimValue);

        /// <summary>
        /// Adds a single, authorized role to the user account.
        /// The user is automatically authenticated when the role is added.
        /// </summary>
        /// <param name="roleName">Name of the role to add.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        public ITestUserSecurityContextBuilder AddUserRole(string roleName);

        /// <summary>
        /// Creates a new user account with the user settings defined in this builder.
        /// </summary>
        /// <returns>ClaimsPrincipal.</returns>
        IUserSecurityContext CreateSecurityContext();
    }
}