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
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this value in the user's query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentEnumSuppliedValue(IDocumentPart parentPart, EnumValueNode node, string key = null)
            : base(parentPart, node, key)
        {
            this.Value = node.Value;
        }

        /// <inheritdoc />
        public ReadOnlyMemory<char> Value { get; }

        /// <inheritdoc />
        public ReadOnlySpan<char> ResolvableValue => this.Value.Span;
    }
}