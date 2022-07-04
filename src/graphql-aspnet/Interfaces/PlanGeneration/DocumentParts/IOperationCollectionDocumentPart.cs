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
    /// A collection of operations defined on a given user supplied query document.
    /// </summary>
    public interface IOperationCollectionDocumentPart : IReadOnlyDictionary<string, IOperationDocumentPart>
    {
        /// <summary>
        /// Gets the query document that owns this collection of operations.
        /// </summary>
        /// <value>The owner.</value>
        IGraphQueryDocument Owner { get; }

        /// <summary>
        /// Gets the <see cref="IOperationDocumentPart"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the operation within the query document.</param>
        /// <returns>IOperationDocumentPart.</returns>
        IOperationDocumentPart this[int index] { get; }
    }
}