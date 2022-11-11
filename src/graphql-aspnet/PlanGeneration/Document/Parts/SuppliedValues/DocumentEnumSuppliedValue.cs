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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// An input value representing a single enumeration value of data.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentEnumSuppliedValue : DocumentSuppliedValue, IEnumSuppliedValueDocumentPart
    {

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