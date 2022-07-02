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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// An input value representing that nothing/null was used as a supplied parameter for an input argument.
    /// </summary>
    internal class DocumentNullSuppliedValue : DocumentSuppliedValue, INullSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNullSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentNullSuppliedValue(IDocumentPart parentPart, SyntaxNode node, string key = null)
            : base(parentPart, node, key)
        {
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            return value != null && value is INullSuppliedValueDocumentPart;
        }

        /// <inheritdoc />
        public ReadOnlySpan<char> ResolvableValue => ReadOnlySpan<char>.Empty;
    }
}