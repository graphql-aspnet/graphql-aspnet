// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.Response
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a dictionary of nested items to be included in a
    /// GraphQL response.
    /// </summary>
    public interface IQueryResponseFieldSet : IQueryResponseItem
    {
        /// <summary>
        /// Gets the dictionary of fields defined for this response item.
        /// </summary>
        /// <value>The fields.</value>
        IReadOnlyDictionary<string, IQueryResponseItem> Fields { get; }
    }
}