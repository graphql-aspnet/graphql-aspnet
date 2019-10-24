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
    /// Represents a dictionary of nested response items.
    /// </summary>
    public interface IResponseFieldSet : IResponseItem
    {
        /// <summary>
        /// Gets the dictionary of fields defined for this response item.
        /// </summary>
        /// <value>The fields.</value>
        IReadOnlyDictionary<string, IResponseItem> Fields { get; }
    }
}