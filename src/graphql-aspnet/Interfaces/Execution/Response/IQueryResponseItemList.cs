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
    /// An item representing a list of items to be included in a
    /// GraphQL response.
    /// </summary>
    public interface IQueryResponseItemList : IQueryResponseItem
    {
        /// <summary>
        /// Gets the list items.
        /// </summary>
        /// <value>The list items.</value>
        IReadOnlyList<IQueryResponseItem> Items { get; }
    }
}