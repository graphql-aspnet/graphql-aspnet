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
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A single field of data from a source object selected to be returned as part of a graph query.
    /// </summary>
    public interface IFieldSelectionDocumentPart : ITargetedDocumentPart, IFieldContainerDocumentPart, IDirectiveContainerDocumentPart, IInputArgumentContainerDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Sets the path of this field to be nested under the supplied parent.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        internal void UpdatePath(SourcePath parentPath);

        /// <summary>
        /// Determines whether this field is capable of resolving itself for the given graph type.
        /// </summary>
        /// <param name="graphType">the graph type to test.</param>
        /// <returns><c>true</c> if this field can be returned the specified graph type; otherwise, <c>false</c>.</returns>
        internal bool CanResolveForGraphType(IGraphType graphType);

        /// <summary>
        /// Gets the path in the document that leads to this field selection.
        /// </summary>
        /// <value>The field selecton path.</value>
        SourcePath Path { get; }

        /// <summary>
        /// Gets the name of the field requested, as it exists in the schema.
        /// </summary>
        /// <value>The parsed name from the queryt document.</value>
        ReadOnlyMemory<char> Name { get; }

        /// <summary>
        /// Gets the alias assigned to the field requested as it was defined in the user's query document.
        /// Defaults to <see cref="Name"/> if not supplied.
        /// </summary>
        /// <value>The parsed alias from the query document.</value>
        ReadOnlyMemory<char> Alias { get; }

        /// <summary>
        /// Gets the graph type returned by this field as indicated by the schema.
        /// </summary>
        /// <value>The graph type of this field.</value>
        IGraphType GraphType { get; }

        /// <summary>
        /// Gets the field reference pointed to by this instance as its declared in the schema.
        /// </summary>
        /// <value>The field.</value>
        IGraphField Field { get; }

        /// <summary>
        /// Gets the node in the parsed syntax tree that generated this instance.
        /// </summary>
        /// <value>The node.</value>
        FieldNode Node { get; }
    }
}