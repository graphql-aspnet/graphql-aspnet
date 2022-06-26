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
        /// Initializes a new instance of the <see cref="DocumentScalarSuppliedValue"/> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this value in the user's query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentScalarSuppliedValue(IDocumentPart parentPart, ScalarValueNode node, string key = null)
            : base(parentPart, node, key)
        {
            this.ValueType = node.ValueType;
            this.Value = node.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentScalarSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="value">The value of the scalar value.</param>
        /// <param name="valueType">The type of scalar value being represented.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentScalarSuppliedValue(IDocumentPart parentPart, string value, ScalarValueType valueType, string key = null)
             : base(parentPart, EmptyNode.Instance, key)
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