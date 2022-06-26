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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A represention of a top level operation (mutation, query etc.) defined in a query document.
    /// </summary>
    public interface IOperationDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gathers the variables currently defined as children of this
        /// operation and packages them into a collection.
        /// </summary>
        /// <returns>IVariableCollectionDocumentPart.</returns>
        IVariableCollectionDocumentPart GatherVariables();

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
    }
}