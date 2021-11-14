// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a single scalar value of data.
    /// </summary>
    [DebuggerDisplay("Scalar: {Value.ToString()} (Type: {ValueType})")]
    public class QueryScalarInputValue : QueryInputValue, IResolvableValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScalarInputValue" /> class.
        /// </summary>
        /// <param name="value">The value parsed from a query document.</param>
        public QueryScalarInputValue(ScalarValueNode value)
            : base(value)
        {
            this.ValueType = value.ValueType;
            this.Value = value.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScalarInputValue"/> class.
        /// </summary>
        /// <param name="node">The node that represents a location in the document where this scalar should be
        /// present.</param>
        /// <param name="value">The value of the scalar value.</param>
        /// <param name="valueType">The type of scalar value being represented.</param>
        public QueryScalarInputValue(SyntaxNode node, string value, ScalarValueType valueType)
             : base(node)
        {
            this.Value = value?.AsMemory() ?? ReadOnlyMemory<char>.Empty;
            this.ValueType = valueType;
        }

        /// <summary>
        /// Gets the type of the value as it was read on the query document.
        /// </summary>
        /// <value>The type of the value.</value>
        public ScalarValueType ValueType { get; }

        /// <summary>
        /// Gets the value literal of the data passed to the input value.
        /// </summary>
        /// <value>The value.</value>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Gets the value to be used to resolve to some .NET type.
        /// </summary>
        /// <value>The resolvable value.</value>
        public ReadOnlySpan<char> ResolvableValue => this.Value.Span;
    }
}