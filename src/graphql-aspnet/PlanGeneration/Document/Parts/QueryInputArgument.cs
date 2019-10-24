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
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("Input Arg: {Name} (GraphType = {GraphType.Name})")]
    public class QueryInputArgument : IDirectiveContainerDocumentPart, IInputValueDocumentPart, IDocumentPart
    {
        private readonly List<(int Rank, QueryDirective Directive)> _rankedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryInputArgument" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="graphType">The expected graph type of the argument value.</param>
        /// <param name="typeExpression">The type expression.</param>
        public QueryInputArgument(InputItemNode node, IGraphType graphType, GraphTypeExpression typeExpression)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Name = node.InputName.ToString();
            _rankedDirectives = new List<(int Rank, QueryDirective Directive)>();
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
        }

        /// <summary>
        /// Inserts the directive into this document part at the head of its directive set.
        /// </summary>
        /// <param name="directive">The directive to add to this instance.</param>
        /// <param name="rank">The relative rank of this directive to others this instance might container.
        /// Directives are executed in ascending order by the engine.</param>
        public void InsertDirective(QueryDirective directive, int rank)
        {
            _rankedDirectives.Add((rank, directive));
        }

        /// <summary>
        /// Assigns the value to this argument as its singular top level value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AssignValue(QueryInputValue value)
        {
            Validation.ThrowIfNull(value, nameof(value));
            value.AssignParent(this);
            this.Value = value;
        }

        /// <summary>
        /// Gets the graph type of this argument.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the node that created this input argument on the query document.
        /// </summary>
        /// <value>The node.</value>
        public SyntaxNode Node { get; }

        /// <summary>
        /// Gets the name of the input argument as it was provided in the query document. (This is a convience property).
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the value of this input argument as it was defined in the query document.
        /// </summary>
        /// <value>The value.</value>
        public QueryInputValue Value { get; private set; }

        /// <summary>
        /// Gets the type expression declared on the schema for this argument. Provides information on how
        /// the data should be provided on a query document.
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var directive in _rankedDirectives.OrderBy(x => x.Rank).Select(x => x.Directive))
                    yield return directive;

                yield return this.Value;
            }
        }

        /// <summary>
        /// Gets a friendly name describing the type of input value this document part represents.
        /// </summary>
        /// <value>The type of the input.</value>
        public string InputType => "argument";
    }
}