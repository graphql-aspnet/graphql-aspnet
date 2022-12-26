// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// An indexed collection of named fragments contained within a query document.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentNamedFragmentCollection : INamedFragmentCollectionDocumentPart
    {
        private OrderedDictionary<string, INamedFragmentDocumentPart> _fragments;

        // its possible that a document declares more than one named fragment with the same
        // name. Keep track of the duplicates for document validation reporting.
        private Dictionary<string, IList<INamedFragmentDocumentPart>> _overflowFragments = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNamedFragmentCollection"/> class.
        /// </summary>
        /// <param name="parent">The document part that owns this instance.</param>
        public DocumentNamedFragmentCollection(IDocumentPart parent)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            _fragments = new OrderedDictionary<string, INamedFragmentDocumentPart>();
        }

        /// <summary>
        /// Adds the fragment to this collection or marks it as a duplicate.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        public void AddFragment(INamedFragmentDocumentPart fragment)
        {
            Validation.ThrowIfNull(fragment, nameof(fragment));

            if (!_fragments.ContainsKey(fragment.Name))
            {
                _fragments.Add(fragment.Name, fragment);
            }
            else
            {
                _overflowFragments = _overflowFragments ?? new Dictionary<string, IList<INamedFragmentDocumentPart>>();
                if (!_overflowFragments.ContainsKey(fragment.Name))
                    _overflowFragments.Add(fragment.Name, new List<INamedFragmentDocumentPart>());

                _overflowFragments[fragment.Name].Add(fragment);
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out INamedFragmentDocumentPart value)
        {
            return _fragments.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool IsUnique(string fragmentName)
        {
            if (fragmentName == null)
                return false;

            return _overflowFragments == null || !_overflowFragments.ContainsKey(fragmentName);
        }

        /// <inheritdoc />
        public void MarkAsReferenced(string fragmentName)
        {
            if (_fragments.TryGetValue(fragmentName, out var namedFrag))
                namedFrag.MarkAsReferenced();

            if (_overflowFragments != null && _overflowFragments.ContainsKey(fragmentName))
            {
                foreach (var frag in _overflowFragments[fragmentName])
                    frag.MarkAsReferenced();
            }
        }

        /// <inheritdoc />
        public INamedFragmentDocumentPart this[string key] => _fragments[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _fragments.Keys;

        /// <inheritdoc />
        public IEnumerable<INamedFragmentDocumentPart> Values => _fragments.Values;

        /// <inheritdoc />
        public int Count => _fragments.Count;

        /// <inheritdoc />
        public IDocumentPart Parent { get; }

        /// <inheritdoc />
        public INamedFragmentDocumentPart this[int index] => _fragments[index];

        /// <inheritdoc />
        public bool ContainsKey(string key) => _fragments.ContainsKey(key);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, INamedFragmentDocumentPart>> GetEnumerator()
        {
            return _fragments.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}