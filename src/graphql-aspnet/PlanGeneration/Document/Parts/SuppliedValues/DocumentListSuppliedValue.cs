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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A representation of a list of other input values for a single argument.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentListSuppliedValue : DocumentSuppliedValue, IListSuppliedValueDocumentPart
    {
        private List<ISuppliedValueDocumentPart> _listItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentListSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentListSuppliedValue(IDocumentPart parentPart, SyntaxNode node, string key = null)
            : base(parentPart, node, key)
        {
            _listItems = new List<ISuppliedValueDocumentPart>();
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IListSuppliedValueDocumentPart))
                return false;

            var valueList = value as IListSuppliedValueDocumentPart;
            if (valueList.ListItems.Count != this.ListItems.Count)
                return false;

            for (var i = 0; i < this.ListItems.Count; i++)
            {
                var localItem = this.ListItems[i];
                var otherItem = valueList.ListItems[i];

                if (!localItem.IsEqualTo(otherItem))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            if (relativeDepth == 1 && childPart is ISuppliedValueDocumentPart svdp)
                _listItems.Add(svdp);
        }

        /// <inheritdoc />
        public IReadOnlyList<ISuppliedValueDocumentPart> ListItems => _listItems;

        /// <inheritdoc />
        public IEnumerator<IResolvableKeyedItem> GetEnumerator() => _listItems.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc />
        public override string Description => $"ListValue (Count = {_listItems.Count})";
    }
}