// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Response
{
    using System.Collections.Generic;

    /// <summary>
    /// An item that is part of a graphql query response representing a list of other items.
    /// </summary>
    public interface IResponseList : IResponseItem
    {
        /// <summary>
        /// Gets the list items.
        /// </summary>
        /// <value>The list items.</value>
        IReadOnlyList<IResponseItem> Items { get; }
    }
}