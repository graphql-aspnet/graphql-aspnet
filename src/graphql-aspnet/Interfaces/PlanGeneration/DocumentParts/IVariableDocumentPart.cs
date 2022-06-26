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
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A document part representing the declaration of a variable
    /// on an operation defined in a query document.
    /// </summary>
    public interface IVariableDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Marks this variable as being referenced within the operation.
        /// </summary>
        internal void MarkAsReferenced();

        /// <summary>
        /// Gets the type expression assigned to this variable.
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the formal name of this variable.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is referenced somewhere within the operation
        /// where its defined.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        bool IsReferenced { get; }

        /// <summary>
        /// Gets the default value assigned to this variable, if any.
        /// </summary>
        /// <value>The default value.</value>
        ISuppliedValueDocumentPart DefaultValue { get; }
    }
}