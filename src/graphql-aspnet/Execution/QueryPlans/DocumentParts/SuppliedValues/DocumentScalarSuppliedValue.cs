// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts.SuppliedValues
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// An input value representing a single scalar value of data.
    /// </summary>
    [DebuggerDisplay("Scalar: {Value.ToString()} (Type: {ValueType})")]
    internal class DocumentScalarSuppliedValue : DocumentSuppliedValue, IScalarSuppliedValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentScalarSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="scalarValue">The scalar value as declared in the source text.</param>
        /// <param name="valueType">The value type of the supplied <paramref name="scalarValue"/>. (e.g. number, string etc.).</param>
        /// <param name="location">The location in the source text where this
        /// document part originated.</param>
        /// <param name="key">A key value uniquely identifying this document part, if any.</param>
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