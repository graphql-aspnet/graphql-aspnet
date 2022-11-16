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
    public enum SchemaItemSecurityChallengeStatus
    {
        /// <summary>
        /// An unknown state, this item indicates an error and unauthorized condition.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates that the challenge failed to complete successfully.
        /// </summary>
        Failed = 10,

        /// <summary>
        /// Indicates that the challenge completed and the user was not authenticated with an expected or required scheme.
        /// </summary>
        Unauthenticated = 20,

        /// <summary>
        /// Indicates that the challenge completed and the user was deemed unauthorized.
        /// </summary>
        Unauthorized = 30,

        /// <summary>
        /// Indicates that no challange occured. THe user was neither authorized
        /// nor unauthorized
        /// </summary>
        Skipped = 100,

        /// <summary>
        /// Inidcates that the challenge completed and the user was authorized.
        /// </summary>
        Authorized = 110,
    }
}