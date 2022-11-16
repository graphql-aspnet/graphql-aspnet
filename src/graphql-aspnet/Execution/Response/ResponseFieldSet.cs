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
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// A collection of keyed items included as a result to a graphql query.
    /// </summary>
    [DebuggerDisplay("Count = {Fields.Count}")]
    public class ResponseFieldSet : IResponseFieldSet
    {
        private readonly OrderedDictionary<string, IResponseItem> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseFieldSet"/> class.
        /// </summary>
        public ResponseFieldSet()
        {
            _dictionary = new OrderedDictionary<string, IResponseItem>();
        }

        /// <summary>
        /// Adds a new response item to this field set with the specified name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="item">The item.</param>
        public void Add(string name, IResponseItem item)
        {
            _dictionary.Add(name, item);
        }

        /// <summary>
        /// Adds the given object as a single value item in this field set.
        /// </summary>
        /// <param name="name">The name to give the item.</param>
        /// <param name="item">The item to add.</param>
        public void AddSingleValue(string name, object item)
        {
            this.Add(name, new ResponseSingleValue(item));
        }

        /// <summary>
        /// Gets the dictionary of fields defined for this response item.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyDictionary<string, IResponseItem> Fields => _dictionary;
    }
}