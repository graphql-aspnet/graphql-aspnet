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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A document part representing the declaration of a variable
    /// on an operation defined in a query document.
    /// </summary>
    public interface IVariableDocumentPart : IDirectiveContainerDocumentPart, IDocumentPart
    {
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
        /// Gets the default value assigned to this variable, if any.
        /// </summary>
        /// <value>The default value.</value>
        ISuppliedValueDocumentPart DefaultValue { get; }
    }
}