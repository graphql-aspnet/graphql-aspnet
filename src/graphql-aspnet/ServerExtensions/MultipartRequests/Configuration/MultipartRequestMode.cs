// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration
{
    using System;

    /// <summary>
    /// An enum detailing what parts of the multi-part request spec are enabled for this schema.
    /// </summary>
    [Flags]
    public enum MultipartRequestMode
    {
        None = 0,
        FileUploads = 1,
        BatchQueries = 2,
        All = FileUploads | BatchQueries,
    }
}