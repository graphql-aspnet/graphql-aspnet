// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A collection of named fragments within a query document.
    /// </summary>
    public interface INamedFragmentCollectionDocumentPart : IReadOnlyDictionary<string, INamedFragmentDocumentPart>
    {
        /// <summary>
        /// Determines whether the specified fragment name is or would be considered unique within this
        /// collection.
        /// </summary>
        /// <param name="fragmentName">Name of the fragment to check.</param>
        /// <returns><c>true</c> if the specified fragment name is unique within the collection; otherwise, <c>false</c>.</returns>
        bool IsUnique(string fragmentName);

        /// <summary>
        /// Gets the parent document part that owns this collection.
        /// </summary>
        /// <value>The parent document part of this collection.</value>
        IDocumentPart Parent { get; }

        /// <summary>
        /// Gets the <see cref="INamedFragmentDocumentPart"/> by the index at which it appears
        /// within the source document.
        /// </summary>
        /// <param name="index">The index of the source document.</param>
        /// <returns>INamedFragmentDocumentPart.</returns>
        INamedFragmentDocumentPart this[int index] { get; }

        /// <summary>
        /// Searches this collection for any named fragments of the matching name
        /// and marks this as "referenced".
        /// </summary>
        /// <param name="fragmentName">Name of the fragment.</param>
        void MarkAsReferenced(string fragmentName);
    }
}