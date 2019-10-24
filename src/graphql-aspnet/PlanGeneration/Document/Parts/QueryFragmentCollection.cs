// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// A collection of fragments parsed from the document that may be referenced by the various operations in the document.
    /// </summary>
    internal class QueryFragmentCollection : Dictionary<string, QueryFragment>
    {
        /// <summary>
        /// Finds the fragment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>QueryFragment.</returns>
        public QueryFragment FindFragment(string name)
        {
            if (this.ContainsKey(name))
                return this[name];
            return null;
        }

        /// <summary>
        /// Adds the node to the collection.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        public void AddFragment(QueryFragment fragment)
        {
            Validation.ThrowIfNull(fragment, nameof(fragment));
            this.Add(fragment.Name, fragment);
        }
    }
}