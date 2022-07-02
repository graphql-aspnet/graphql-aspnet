// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common
{
    using System.Collections.Generic;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// An collection of document parts with a common owner.
    /// </summary>
    public interface IDocumentPartsCollection : IReadOnlyDocumentPartsCollection
    {
        /// <summary>
        /// Raised when a new direct child part is added to this collection or any of its
        /// child's collection (recursive).
        /// </summary>
        internal event DocumentCollectionAlteredHandler ChildPartAdded;

        /// <summary>
        /// Adds the document parts to this collection. The parts
        /// must have a parent pointing to the owner of this collection.
        /// </summary>
        /// <param name="documentParts">The document parts to add.</param>
        void Add(params IDocumentPart[] documentParts);

        /// <summary>
        /// Adds the document parts to this collection. The parts
        /// must have a parent pointing to the owner of this collection.
        /// </summary>
        /// <param name="documentParts">The document parts to add.</param>
        void Add(IEnumerable<IDocumentPart> documentParts);
    }
}