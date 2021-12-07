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
        /// Authentication failed, no user credentials were created.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Authentication was successful, user credentials were created.
        /// </summary>
        Success = 3,
    }
}