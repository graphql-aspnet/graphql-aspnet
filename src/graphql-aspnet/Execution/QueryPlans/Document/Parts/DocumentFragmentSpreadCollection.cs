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
        private Dictionary<string, List<IFragmentSpreadDocumentPart>> _spreads;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentSpreadCollection"/> class.
        /// </summary>
        /// <param name="owner">The document part that owns this set of fragment spreads.</param>
        public DocumentFragmentSpreadCollection(IDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _spreads = new Dictionary<string, List<IFragmentSpreadDocumentPart>>();
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
                var fragName = fragmentSpread.FragmentName.ToString();
                if (!_spreads.ContainsKey(fragName))
                    _spreads.Add(fragName, new List<IFragmentSpreadDocumentPart>());

                _spreads[fragName].Add(fragmentSpread);
                this.Count += 1;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFragmentSpreadDocumentPart> FindReferences(string fragmentName)
        {
            fragmentName = Validation.ThrowIfNullWhiteSpaceOrReturn(fragmentName, nameof(fragmentName));
            if (_spreads.ContainsKey(fragmentName))
                return _spreads[fragmentName];

            return Enumerable.Empty<IFragmentSpreadDocumentPart>();
        }

        /// <inheritdoc />
        public bool IsSpread(string fragmentName)
        {
            fragmentName = Validation.ThrowIfNullWhiteSpaceOrReturn(fragmentName, nameof(fragmentName));
            return _spreads.ContainsKey(fragmentName);
        }

        /// <inheritdoc />
        public IEnumerator<IFragmentSpreadDocumentPart> GetEnumerator()
        {
            return _spreads.Values.SelectMany(x => x).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        public IDocumentPart Owner { get; }
    }
}