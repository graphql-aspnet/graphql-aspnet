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
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("Input Arg: {Name} (GraphType = {GraphType.Name})")]
    public class DocumentInputArgument : IQueryArgumentDocumentPart
    {
        private readonly List<(int Rank, IDirectiveDocumentPart Directive)> _rankedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgument" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="graphType">The expected graph type of the argument value.</param>
        /// <param name="typeExpression">The type expression.</param>
        public DocumentInputArgument(InputItemNode node, IGraphType graphType, GraphTypeExpression typeExpression)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Name = node.InputName.ToString();
            _rankedDirectives = new List<(int Rank, IDirectiveDocumentPart Directive)>();
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
        }

        /// <inheritdoc />
        public void InsertDirective(IDirectiveDocumentPart directive, int rank)
        {
            _rankedDirectives.Add((rank, directive));
        }

        /// <inheritdoc />
        public void AssignValue(ISuppliedValueDocumentPart value)
        {
            Validation.ThrowIfNull(value, nameof(value));
            value.Owner = this;
            this.Value = value;
        }

        /// <inheritdoc />
        public IGraphType GraphType { get; }

        /// <inheritdoc />
        public SyntaxNode Node { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ISuppliedValueDocumentPart Value { get; private set; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var directive in _rankedDirectives.OrderBy(x => x.Rank).Select(x => x.Directive))
                    yield return directive;

                yield return this.Value;
            }
        }

        /// <inheritdoc />
        public string InputType => this.PartType.ToString();

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.InputArgument;
    }
}