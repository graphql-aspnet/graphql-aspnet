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
        /// Adds the document part to this collection. The part
        /// must have a parent pointing to the owner of this collection.
        /// </summary>
        /// <param name="documentPart">The document part to add.</param>
        void Add(IDocumentPart documentPart);
    }
}