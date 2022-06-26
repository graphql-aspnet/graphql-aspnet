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
    using GraphQL.AspNet.PlanGeneration.Document;

    /// <summary>
    /// A readonly collection of document parts. All parts in this collection are syblings
    /// in a document hierarchy.
    /// </summary>
    public interface IReadOnlyDocumentPartsCollection : IReadOnlyList<IDocumentPart>
    {
        /// <summary>
        /// Gets the set of parts within this collection of the specified type. If no parts
        /// are found an empty list is returned.
        /// </summary>
        /// <param name="partType">Type of the part to return.</param>
        /// <returns>IReadOnlyList&lt;IDocumentPart&gt;.</returns>
        IReadOnlyList<IDocumentPart> this[DocumentPartType partType] { get; }

        /// <summary>
        /// Gets the part type that owns this collection.
        /// </summary>
        /// <value>The owner.</value>
        IDocumentPart Owner { get; }
    }
}