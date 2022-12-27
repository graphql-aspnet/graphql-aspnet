// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Response
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution.Response;

    /// <summary>
    /// A collection of items generated as part of a response to a graphql
    /// query.
    /// </summary>
    [DebuggerDisplay("Count = {_list.Count}")]
    internal class ResponseList : IQueryResponseItemList
    {
        private List<IQueryResponseItem> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseList"/> class.
        /// </summary>
        /// <param name="items">The items to include in this collection.</param>
        public ResponseList(IEnumerable<IQueryResponseItem> items = null)
        {
            _list = items != null ? new List<IQueryResponseItem>(items) : new List<IQueryResponseItem>();
        }

        /// <summary>
        /// Adds the specified item to the growing list.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(IQueryResponseItem item)
        {
            _list.Add(item);
        }

        /// <summary>
        /// Gets the list items.
        /// </summary>
        /// <value>The list items.</value>
        public IReadOnlyList<IQueryResponseItem> Items => _list;
    }
}