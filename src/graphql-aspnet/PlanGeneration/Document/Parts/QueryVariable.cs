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
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A semi-parsed reference to a variable on an operation used during document validation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Variable: {Name}")]
    public class QueryVariable : IDocumentPart, IInputValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVariable" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public QueryVariable(VariableNode node)
        {
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Name = node.Name.ToString();
            this.TypeExpression = GraphTypeExpression.FromDeclaration(node.TypeExpression.Span);
            this.UsedByArguments = new List<QueryInputArgument>();
        }

        /// <summary>
        /// Attaches the found graph type to this instance.
        /// </summary>
        /// <param name="graphType">The found graph type this variable references.</param>
        public void AttachGraphType(IGraphType graphType)
        {
            this.GraphType = graphType;
        }

        /// <summary>
        /// Marks this variable as being referenced within the operation.
        /// </summary>
        public void MarkAsReferenced()
        {
            this.IsReferenced = true;
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
        /// Gets a value indicating whether this instance is referenced somewhere within the operation
        /// where its defined.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        public bool IsReferenced { get; private set; }

        /// <summary>
        /// Gets the node this variable was created from.
        /// </summary>
        /// <value>The node.</value>
        public SyntaxNode Node { get; }

        /// <summary>
        /// Gets the name of the variable as its defined for the operation.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the type expression that represents the data of this argument (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <summary>
        /// Gets the core graph type this variable is declared as.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphType GraphType { get; private set; }

        /// <summary>
        /// Gets the set of arguments where this variable has been referenced.
        /// </summary>
        /// <value>The arguments.</value>
        public IList<QueryInputArgument> UsedByArguments { get; }

        /// <summary>
        /// Gets the default value assigned to this variable in the query document, if any.
        /// </summary>
        /// <value>The default value.</value>
        public QueryInputValue Value { get; private set; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();

        /// <summary>
        /// Gets a friendly name describing the type of input value this document part represents.
        /// </summary>
        /// <value>The type of the input.</value>
        public string InputType => "variable";
    }
}