// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Response
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// A collection of items generated as part of a response to a graphql
    /// query.
    /// </summary>
    [DebuggerDisplay("Count = {_list.Count}")]
    public class ResponseList : IResponseList
    {
        private List<IResponseItem> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseList"/> class.
        /// </summary>
        /// <param name="items">The items to include in this collection.</param>
        public ResponseList(IEnumerable<IResponseItem> items = null)
        {
            _list = items != null ? new List<IResponseItem>(items) : new List<IResponseItem>();
        }

        /// <summary>
        /// Adds the specified item to the growing list.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(IResponseItem item)
        {
            _list.Add(item);
        }

        /// <summary>
        /// Gets the list items.
        /// </summary>
        /// <value>The list items.</value>
        public IReadOnlyList<IResponseItem> Items => _list;
    }
}