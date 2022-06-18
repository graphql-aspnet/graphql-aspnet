// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using System.Collections.Generic;

    /// <summary>
    /// The collection of named fragments defined on a query operation.
    /// </summary>
    public interface IFragmentCollectionDocumentPart : IReadOnlyDictionary<string, IFragmentDocumentPart>, IDocumentPart
    {
        /// <summary>
        /// Finds a fragment in the collection by its given name.
        /// </summary>
        /// <param name="name">The name of the fragment to search for.</param>
        /// <returns>QueryFragment.</returns>
        IFragmentDocumentPart FindFragment(string name);

        /// <summary>
        /// Adds the fragment to this collection.
        /// </summary>
        /// <param name="fragment">The fragment to add.</param>
        internal void AddFragment(IFragmentDocumentPart fragment);
    }
}