// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Security
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A context representing the user credentials to be authenticated and used
    /// for this graphql request.
    /// </summary>
    public interface IUserSecurityContext
    {
        /// <summary>
        /// Authenticates the user credentials contained within this security context using
        /// the supplied authentication scheme.
        /// </summary>
        /// <param name="scheme">The id/key value of the scheme to authenticate with.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IAuthenticationResult&gt;.</returns>
        Task<IAuthenticationResult> Authenticate(string scheme, CancellationToken token = default);

        /// <summary>
        /// Authenticates the user credentials contained within this security context using
        /// a pre-configured, default authentication scheme.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IAuthenticationResult&gt;.</returns>
        Task<IAuthenticationResult> Authenticate(CancellationToken token = default);

        /// <summary>
        /// <para>
        /// Gets the claims principal representing the user as supplied by default
        /// from the underlying security management medium.
        /// </para>
        /// </summary>
        /// <value>The user.</value>
        ClaimsPrincipal DefaultUser { get; }
    }
}