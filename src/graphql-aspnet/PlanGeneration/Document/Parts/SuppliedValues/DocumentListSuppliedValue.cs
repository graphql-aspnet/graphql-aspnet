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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A representation of a list of other input values for a single argument.
    /// </summary>
    [DebuggerDisplay("ListValue (Count = {Count})")]
    internal class DocumentListSuppliedValue : DocumentSuppliedValue, IListSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentListSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentListSuppliedValue(IDocumentPart parentPart, SyntaxNode node, string key = null)
            : base(parentPart, node, key)
        {
        }

        private int Count => this.ListItems.Count();

        /// <inheritdoc />
        public IEnumerable<ISuppliedValueDocumentPart> ListItems => this
            .Children[DocumentPartType.SuppliedValue]
            .OfType<ISuppliedValueDocumentPart>();

        /// <inheritdoc />
        public IEnumerator<IResolvableKeyedItem> GetEnumerator()
        {
            return this.ListItems.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}