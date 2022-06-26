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
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// An collection of document parts with a common owner.
    /// </summary>
    public interface IDocumentPartsCollection : IReadOnlyDocumentPartsCollection
    {
        /// <summary>
        /// Raised when a new part is added to this collection.
        /// </summary>
        internal event DocumentCollectionAlteredHandler PartAdded;

        /// <summary>
        /// Raised when a part is successfully removed from this collection.
        /// </summary>
        internal event DocumentCollectionAlteredHandler PartRemoved;

        /// <summary>
        /// Raised just before a part is added to this collection.
        /// </summary>
        internal event DocumentCollectionBeforeAddHandler BeforePartAdded;

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

        /// <summary>
        /// Removes the part from this collection if it exists.
        /// </summary>
        /// <param name="part">The part to remove.</param>
        void RemoveChild(IDocumentPart part);
    }
}