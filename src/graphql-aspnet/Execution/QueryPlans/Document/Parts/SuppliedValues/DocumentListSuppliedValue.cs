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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A representation of a list of other input values for a single argument.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentListSuppliedValue : DocumentSuppliedValue, IListSuppliedValueDocumentPart, IDescendentDocumentPartSubscriber
    {
        private List<ISuppliedValueDocumentPart> _listItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentListSuppliedValue"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="location">The location in the source text where
        /// this list originated.</param>
        /// <param name="key">A key value identifying this instance in the document, if any.</param>
        public DocumentListSuppliedValue(IDocumentPart parentPart, SourceLocation location, string key = null)
            : base(parentPart, location, key)
        {
            _listItems = new List<ISuppliedValueDocumentPart>();
            this.DetermineListItemTypeExpression();
        }

        private void DetermineListItemTypeExpression()
        {
            GraphTypeExpression parentExpression = null;
            switch (this.Parent)
            {
                case IInputValueDocumentPart ivdp:
                    parentExpression = ivdp.TypeExpression;
                    break;

                case IListSuppliedValueDocumentPart lsv:
                    parentExpression = lsv.ListItemTypeExpression;
                    break;
            }

            if (parentExpression != null)
            {
                // could be a "non-nullable list"
                // strip the list requirement to determine that the parent is a list
                // of things then take the internal of that as the item type expression
                if (parentExpression.IsNonNullable)
                    parentExpression = parentExpression.UnWrapExpression();

                if (parentExpression.IsListOfItems)
                {
                    this.ListItemTypeExpression = parentExpression.UnWrapExpression();
                }
            }
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

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart.Parent == this && decendentPart is ISuppliedValueDocumentPart svdp)
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

        /// <inheritdoc />
        public GraphTypeExpression ListItemTypeExpression { get; private set; }
    }
}