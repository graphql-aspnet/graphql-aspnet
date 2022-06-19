// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a single scalar value of data.
    /// </summary>
    [DebuggerDisplay("Scalar: {Value.ToString()} (Type: {ValueType})")]
    internal class DocumentScalarSuppliedValue : DocumentSuppliedValue, IScalarSuppliedValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentScalarSuppliedValue" /> class.
        /// </summary>
        /// <param name="value">The value parsed from a query document.</param>
        public DocumentScalarSuppliedValue(ScalarValueNode value)
            : base(value)
        {
            this.ValueType = value.ValueType;
            this.Value = value.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentScalarSuppliedValue"/> class.
        /// </summary>
        /// <param name="node">The node that represents a location in the document where this scalar should be
        /// present.</param>
        /// <param name="value">The value of the scalar value.</param>
        /// <param name="valueType">The type of scalar value being represented.</param>
        public DocumentScalarSuppliedValue(SyntaxNode node, string value, ScalarValueType valueType)
             : base(node)
        {
            this.Value = value?.AsMemory() ?? ReadOnlyMemory<char>.Empty;
            this.ValueType = valueType;
        }

        /// <inheritdoc />
        public ScalarValueType ValueType { get; }

        /// <inheritdoc />
        public ReadOnlyMemory<char> Value { get; }

        /// <inheritdoc />
        public ReadOnlySpan<char> ResolvableValue => this.Value.Span;
    }
}