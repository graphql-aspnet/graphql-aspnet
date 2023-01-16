// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System.Collections.Generic;

    /// <summary>
    /// A collection of queries to execute and file uploads parsed by the graphql-multipart-request specification.
    /// </summary>
    public class MultipartGraphQlHttpPayload
    {
        public MultipartGraphQlHttpPayload(List<GraphQueryData> queryData)
        {

        }
    }
}