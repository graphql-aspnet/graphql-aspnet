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
    public enum FieldAuthorizationStatus
    {
        Skipped,
        Unauthorized,
        Authorized,
    }
}