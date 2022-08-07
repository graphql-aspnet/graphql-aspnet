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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A represention of a top level operation (mutation, query etc.) defined in a query document.
    /// </summary>
    public interface IOperationDocumentPart
        : IDirectiveContainerDocumentPart, ITopLevelDocumentPart, IReferenceDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Gets the type of the operation that was parsed.
        /// </summary>
        /// <value>The type of the operation.</value>
        GraphOperationType OperationType { get; }

        /// <summary>
        /// Gets the name assigned to this query operation. Used to distinguish, and required for,
        /// documents containing multiple operations that may be executed.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the name of the operation type as it was declared in the document
        /// (e.g. query, mutation subscription etc.)
        /// </summary>
        /// <value>The name of the operation type.</value>
        string OperationTypeName { get; }

        /// <summary>
        /// Gets the defined field selection set for this operation.
        /// </summary>
        /// <value>The field selection set.</value>
        IFieldSelectionSetDocumentPart FieldSelectionSet { get; }

        /// <summary>
        /// Gets the variables declared in this operation.
        /// </summary>
        /// <value>The variables.</value>
        IVariableCollectionDocumentPart Variables { get; }

        /// <summary>
        /// Gets the depth of any field of this operation, including spread
        /// named fragments.
        /// </summary>
        /// <value>The depth achived by the operation.</value>
        int MaxDepth { get; }

        /// <summary>
        /// Gets a list of all the document parts in this operation that represent
        /// a secured item (typically a field or directive).
        /// </summary>
        /// <value>The secure items.</value>
        IReadOnlyList<ISecureDocumentPart> SecureItems { get; }
    }
}