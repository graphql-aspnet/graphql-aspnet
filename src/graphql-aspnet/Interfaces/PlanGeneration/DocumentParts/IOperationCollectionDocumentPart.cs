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
    /// An interface representing a collection of operations read from a query document.
    /// </summary>
    public interface IOperationCollectionDocumentPart : IReadOnlyDictionary<string, IOperationDocumentPart>, IDocumentPart
    {
        /// <summary>
        /// Adds the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        internal void AddOperation(IOperationDocumentPart operation);

        /// <summary>
        /// Adds the set of <see cref="IOperationDocumentPart"/> to this collection, keyed by the name
        /// provided in the user query document.
        /// </summary>
        /// <param name="operations">The set of operations to add.</param>
        internal void AddRange(IEnumerable<IOperationDocumentPart> operations);
    }
}