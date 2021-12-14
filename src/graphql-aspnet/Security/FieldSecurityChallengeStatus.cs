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
    /// A set of possible stati indicating the success or failure
    /// of a pipeline authorization challenge.
    /// </summary>
    public enum FieldSecurityChallengeStatus
    {
        /// <summary>
        /// Indicates that no challange occured. THe user was neither authorized
        /// nor unauthorized
        /// </summary>
        Skipped,

        /// <summary>
        /// Indicates that the challenge failed to complete successfully.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates that the challenge completed and the user was not authenticated with an expected or required scheme.
        /// </summary>
        Unauthenticated,

        /// <summary>
        /// Indicates that the challenge completed and the user was deemed unauthorized.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// Inidcates that the challenge completed and the user was authorized.
        /// </summary>
        Authorized,
    }
}