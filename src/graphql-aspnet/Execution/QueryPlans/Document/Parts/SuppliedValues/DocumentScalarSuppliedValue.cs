// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// An input value representing a single scalar value of data.
    /// </summary>
    [DebuggerDisplay("Scalar: {Value.ToString()} (Type: {ValueType})")]
    internal class DocumentScalarSuppliedValue : DocumentSuppliedValue, IScalarSuppliedValue
    {
        public DocumentScalarSuppliedValue(
            IDocumentPart parentPart,
            string scalarValue,
            ScalarValueType valueType,
            SourceLocation location,
            string key = null)
            : base(parentPart, location, key)
        {
            this.ValueType = valueType;
            this.Value = scalarValue;
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IScalarSuppliedValue))
                return false;

            var otherValue = value as IScalarSuppliedValue;
            return this.Value == otherValue.Value;
        }

        /// <inheritdoc />
        public ScalarValueType ValueType { get; }

        /// <inheritdoc />
        public string Value { get; }

        /// <inheritdoc />
        public ReadOnlySpan<char> ResolvableValue => this.Value.AsSpan();

        /// <inheritdoc />
        public override string Description => "SCALAR: " + this.Value.ToString();
    }
}