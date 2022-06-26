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
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// Represents an input argument to a field, directive or complex value declared
    /// in a query document.
    /// </summary>
    public interface IInputArgumentDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets the child of this argument that represents a value to be resolved.
        /// </summary>
        /// <value>The value.</value>
        ISuppliedValueDocumentPart Value { get; }

        /// <summary>
        /// Gets the name of this input value part in the query document.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the type expression assigned to this document part.
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }
    }
}