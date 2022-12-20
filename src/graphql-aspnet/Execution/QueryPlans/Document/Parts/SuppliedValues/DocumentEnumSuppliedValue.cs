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
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// An input value representing a single enumeration value of data.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentEnumSuppliedValue : DocumentSuppliedValue, IEnumSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEnumSuppliedValue"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="enumValue">The enum value parsed from the soruce text.</param>
        /// <param name="location">The location in the source text where this part originated.</param>
        /// <param name="key">The key.</param>
        public DocumentEnumSuppliedValue(IDocumentPart parentPart, string enumValue, SourceLocation location, string key = null)
            : base(parentPart, location, key)
        {
            this.Value = enumValue;
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IEnumSuppliedValueDocumentPart))
                return false;

            return this.Value == ((IEnumSuppliedValueDocumentPart)value).Value;
        }

        /// <inheritdoc />
        public string Value { get; }

        /// <inheritdoc />
        public ReadOnlySpan<char> ResolvableValue => this.Value.AsSpan();

        /// <inheritdoc />
        public override string Description => $"EnumValue: {this.Value}";
    }
}