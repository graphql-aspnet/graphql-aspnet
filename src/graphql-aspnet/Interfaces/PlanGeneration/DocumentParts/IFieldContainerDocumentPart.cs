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
    /// <summary>
    /// Represents a document part that can contain child fields.
    /// </summary>
    public interface IFieldContainerDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Creates this field's child selection set. Prior to calling this method this instance
        /// can never have children.
        /// </summary>
        /// <returns>FieldSelectionSet.</returns>
        internal IFieldSelectionSetDocumentPart CreateFieldSelectionSet();

        /// <summary>
        /// Gets the set of nodes requested from this operation. This value is null
        /// until <see cref="CreateFieldSelectionSet()"/> is called.
        /// </summary>
        /// <value>The complete collection of nodes.</value>
        IFieldSelectionSetDocumentPart FieldSelectionSet { get; }
    }
}