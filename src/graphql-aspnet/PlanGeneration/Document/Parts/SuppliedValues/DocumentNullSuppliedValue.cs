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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// An input value representing that nothing/null was used as a supplied parameter for an input argument.
    /// </summary>
    public class DocumentNullSuppliedValue : DocumentSuppliedValue, INullSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNullSuppliedValue" /> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        public DocumentNullSuppliedValue(SyntaxNode node)
            : base(node)
        {
        }

        /// <summary>
        /// Gets the value to be used to resolve to some .NET type.
        /// </summary>
        /// <value>The resolvable value.</value>
        public ReadOnlySpan<char> ResolvableValue => ReadOnlySpan<char>.Empty;
    }
}