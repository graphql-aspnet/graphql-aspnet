// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Web
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A payload representing a parsed <see cref="HttpContext"/> containing
    /// data to be submitted to the query engine for processing.
    /// </summary>
    public interface IHttpGraphQueryPayload : IEnumerable<GraphQueryData>
    {
        /// <summary>
        /// Gets the total number of queryies to process.
        /// </summary>
        /// <value>The number of queries to process.</value>
        int Count { get; }
    }
}