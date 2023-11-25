// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper methods for <see cref="SchemaItemPathCollections"/>.
    /// </summary>
    public static class SchemaItemCollectionsExtensions
    {
        /// <summary>
        /// Determines whether the collection represents an internal collection
        /// of items, noth pathed on the schema.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if [is internal collection] [the specified collection]; otherwise, <c>false</c>.</returns>
        public static bool IsInternalCollection(this SchemaItemPathCollections collection)
        {
            return (int)collection < 0;
        }
    }
}