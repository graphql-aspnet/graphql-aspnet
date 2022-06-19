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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a single enumeration value of data.
    /// </summary>
    [DebuggerDisplay("EnumValue: {Value.ToString()}")]
    internal class DocumentEnumSuppliedValue : DocumentSuppliedValue, IEnumSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEnumSuppliedValue" /> class.
        /// </summary>
        /// <param name="value">The value parsed from a query document.</param>
        public DocumentEnumSuppliedValue(EnumValueNode value)
            : base(value)
        {
            this.Value = value.Value;
        }

        /// <inheritdoc />
        public ReadOnlyMemory<char> Value { get; }

        /// <inheritdoc />
        public ReadOnlySpan<char> ResolvableValue => this.Value.Span;
    }
}