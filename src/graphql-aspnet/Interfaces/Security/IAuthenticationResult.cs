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
    using GraphQL.AspNet.Security;

    /// <summary>
    /// The result of executing an authentication request against
    /// a <see cref="IUserSecurityContext"/>.
    /// </summary>
    public interface IAuthenticationResult
    {
        /// <summary>
        /// Gets a value indicating whether authentication succeeded.
        /// </summary>
        /// <value><c>true</c> if authentication suceeded; otherwise, <c>false</c>.</value>
        bool Suceeded { get; }

        /// <summary>
        /// Gets a string indicating which authentication scheme was used
        /// to authentication the <see cref="User"/>. May not be implemented by all providers.
        /// </summary>
        /// <value>The authentication scheme.</value>
        string AuthenticationScheme { get; }

        /// <summary>
        /// Gets the user that was authenticated. Only populated if <see cref="Suceeded"/>
        /// is <c>true</c>.
        /// </summary>
        /// <value>The user.</value>
        ClaimsPrincipal User { get; }
    }
}