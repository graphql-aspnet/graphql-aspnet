// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Security
{
    /// <summary>
    /// The potential result of completing a user authentication request.
    /// </summary>
    public enum AuthenticationStatus
    {
        /// <summary>
        /// Indicates no state, no opertaion has taken place yet.
        /// </summary>
        None = 0,

        /// <summary>
        /// Authentication was skipped or otherwise intentionally not performed.
        /// </summary>
        Skipped = 1,

        /// <summary>
        /// Authentication failed, no user credentials were created.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Authentication was successful, user credentials were created.
        /// </summary>
        Success = 3,
    }
}