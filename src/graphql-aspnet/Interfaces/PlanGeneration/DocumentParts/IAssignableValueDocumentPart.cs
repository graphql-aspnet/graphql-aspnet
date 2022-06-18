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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A part of a query document that can have a value assigned to it (i.e. an
    /// input argument on a field or an operation variable).
    /// </summary>
    public interface IAssignableValueDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Assigns the value to this argument as its singular top level value.
        /// </summary>
        /// <param name="value">The value to assign to this intance.</param>
        internal void AssignValue(ISuppliedValueDocumentPart value);

        /// <summary>
        /// Gets a friendly name describing the type of input value this document part represents.
        /// </summary>
        /// <value>The type of the input.</value>
        string InputType { get; }

        /// <summary>
        /// Gets the graph type, from the schema, of this argument.
        /// </summary>
        /// <value>The type of the graph.</value>
        IGraphType GraphType { get; }

        /// <summary>
        /// Gets the node in the parsed AST that references this document part in the query document.
        /// </summary>
        /// <value>The node.</value>
        SyntaxNode Node { get; }

        /// <summary>
        /// Gets the value assigned to this document part.
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