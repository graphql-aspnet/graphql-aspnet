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
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A framework component used to build authorization policies the test server will
    /// support.
    /// </summary>
    public interface ITestAuthorizationBuilder : IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// Disables the authorization provider, no provider and no policies
        /// will not be injected into the server.
        /// </summary>
        /// <returns>TestAuthorizationBuilder.</returns>
        ITestAuthorizationBuilder DisableAuthorization();

        /// <summary>
        /// Force updates the <see cref="IAuthorizationService"/> served via DI
        /// to the provided instance for all requests. This method has no effect if
        /// <see cref="DisableAuthorization"/> is called.
        /// </summary>
        /// <param name="newService">The new authorization service to use.</param>
        /// <returns>ITestAuthorizationBuilder.</returns>
        ITestAuthorizationBuilder ReplaceAuthorizationService(IAuthorizationService newService);

        /// <summary>
        /// Adds a simple new policy to the service based on a list of roles. When enforcing this policy
        /// the authorization service will require an authenticated user and
        /// check to see if the user belongs to any role in the list.
        /// </summary>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="roleList">A comma-sepeated list of roles.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        ITestAuthorizationBuilder AddRolePolicy(string policyName, string roleList);

        /// <summary>
        /// Adds a simple new policy to the service requiring that the user be authenticated
        /// and have the given claim type and value pair.
        /// </summary>
        /// <param name="policyName">The name of the policy.</param>
        /// <param name="claimType">The claim type to require.</param>
        /// <param name="claimValue">The claim value to require.</param>
        /// <returns>TestAuthorizationBuilder.</returns>
        ITestAuthorizationBuilder AddClaimPolicy(string policyName, string claimType, string claimValue);

        /// <summary>
        /// Adds a default policy to the <see cref="IAuthorizationService" /> service, mimicing an action similar to calling
        /// <c>AddAuthorization</c> and setting a <c>DefaultPolicy</c> during startup.
        /// </summary>
        /// <returns>TestAuthorizationBuilder.</returns>
        AuthorizationPolicyBuilder DefaultPolicy();

        /// <summary>
        /// Creates a new policy via a raw builder allowing for the testing of an open ended
        /// policy.
        /// </summary>
        /// <param name="policyName">The unique name to give to this policy.</param>
        /// <returns>AuthorizationPolicyBuilder.</returns>
        AuthorizationPolicyBuilder NewPolicy(string policyName);
    }
}