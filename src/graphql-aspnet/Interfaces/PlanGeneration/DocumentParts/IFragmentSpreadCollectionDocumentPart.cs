namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    public interface IFragmentSpreadCollectionDocumentPart : IEnumerable<IFragmentSpreadDocumentPart>
    {
        /// <summary>
        /// Determines whether the specified named fragment is spread by any item in this collection.
        /// </summary>
        /// <param name="fragmentName">Name of the fragment.</param>
        /// <returns><c>true</c> if the specified named fragment is spread by this collection; otherwise, <c>false</c>.</returns>
        bool IsSpread(string fragmentName);


        /// <summary>
        /// Finds the set of fragment spreads which reference the nsupplied named fragment.
        /// </summary>
        /// <param name="fragmentName">Name of the fragment to search for.</param>
        /// <returns>IEnumerable&lt;IFragmentSpreadDocumentPart&gt;.</returns>
        IEnumerable<IFragmentSpreadDocumentPart> FindReferences(string fragmentName);

        /// <summary>
        /// Gets the total number of fragment spreads in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets the owner of this collection.
        /// </summary>
        /// <value>The owner.</value>
        IDocumentPart Owner { get; }
    }
}
