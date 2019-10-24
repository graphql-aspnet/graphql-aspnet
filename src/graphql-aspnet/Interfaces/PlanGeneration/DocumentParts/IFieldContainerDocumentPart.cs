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
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// Represents a document part that can contain child fields.
    /// </summary>
    public interface IFieldContainerDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Creates the field selection set to return from the operation type.
        /// </summary>
        /// <returns>FieldSelectionSet.</returns>
        FieldSelectionSet CreateFieldSelectionSet();

        /// <summary>
        /// Gets the set of nodes requested from this operation.
        /// </summary>
        /// <value>The complete collection of nodes.</value>
        FieldSelectionSet FieldSelectionSet { get; }
    }
}