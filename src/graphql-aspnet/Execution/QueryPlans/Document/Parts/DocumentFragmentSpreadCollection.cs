// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A collection of fragment speads, indexed by the named fragment they are spreading. Used
    /// for easy lookup when validating references during document construction..
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentFragmentSpreadCollection : IFragmentSpreadCollectionDocumentPart
    {
        private List<IFragmentSpreadDocumentPart> _allSpreads;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentSpreadCollection"/> class.
        /// </summary>
        /// <param name="owner">The document part that owns this set of fragment spreads.</param>
        public DocumentFragmentSpreadCollection(IDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _allSpreads = new List<IFragmentSpreadDocumentPart>();
        }

        /// <summary>
        /// Adds the fragment spread to this collection.
        /// </summary>
        /// <param name="fragmentSpread">The fragment spread.</param>
        public void Add(IFragmentSpreadDocumentPart fragmentSpread)
        {
            Validation.ThrowIfNull(fragmentSpread, nameof(fragmentSpread));
            if (fragmentSpread.FragmentName.Length > 0)
            {
                _allSpreads.Add(fragmentSpread);
            }
        }

        /// <inheritdoc />
        public IEnumerator<IFragmentSpreadDocumentPart> GetEnumerator()
        {
            return _allSpreads.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _allSpreads.Count;

        /// <inheritdoc />
        public IDocumentPart Owner { get; }

        /// <inheritdoc />
        public IFragmentSpreadDocumentPart this[int index] => _allSpreads[index];
    }
}