// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *****************************************************************************
// This PooledList has been adapted from jtmueller's implementation: <br/>
// https://github.com/jtmueller/Collections.Pooled/tree/master/Collections.Pooled <br/>
// *****************************************************************************

namespace GraphQL.AspNet.Common.Generics.Pooled
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A view window for debugging collections.
    /// </summary>
    /// <typeparam name="T">The data type in the collection.</typeparam>
    internal sealed class ICollectionDebugView<T>
    {
        private readonly ICollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ICollectionDebugView{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="System.ArgumentNullException">collection</exception>
        public ICollectionDebugView(ICollection<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        /// <summary>
        /// Gets the items to be viewed.
        /// </summary>
        /// <value>The items.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}