// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A semi-parsed reference to a variable on an operation used during document validation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Variable: {Name}")]
    public class DocumentVariable : IQueryVariableDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariable" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public DocumentVariable(VariableNode node)
        {
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Name = node.Name.ToString();
            this.TypeExpression = GraphTypeExpression.FromDeclaration(node.TypeExpression.Span);
        }

        /// <summary>
        /// Attaches the found graph type to this instance.
        /// </summary>
        /// <param name="graphType">The found graph type this variable references.</param>
        public void AttachGraphType(IGraphType graphType)
        {
            this.GraphType = graphType;
        }

        /// <inheritdoc />
        public void MarkAsReferenced()
        {
            this.IsReferenced = true;
        }

        /// <inheritdoc />
        public void AssignValue(ISuppliedValueDocumentPart value)
        {
            Validation.ThrowIfNull(value, nameof(value));
            value.Owner = this;
            this.Value = value;
        }

        /// <inheritdoc />
        public bool IsReferenced { get; private set; }

        /// <inheritdoc />
        public SyntaxNode Node { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <inheritdoc />
        public IGraphType GraphType { get; private set; }

        /// <inheritdoc />
        public ISuppliedValueDocumentPart Value { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();

        /// <inheritdoc />
        public string InputType => this.PartType.ToString();

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.Variable;
    }
}