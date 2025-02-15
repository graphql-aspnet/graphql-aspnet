﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    using OrderedDictionaryOfStringAndNamedFragment = GraphQL.AspNet.Common.Generics.OrderedDictionary<string, GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.INamedFragmentDocumentPart>;

    /// <summary>
    /// An indexed collection of named fragments contained within a query document.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentNamedFragmentCollection : INamedFragmentCollectionDocumentPart
    {
        private OrderedDictionaryOfStringAndNamedFragment _fragments;

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
            _fragments = new OrderedDictionaryOfStringAndNamedFragment();
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
        public void AddSpreadReference(IFragmentSpreadDocumentPart spreadPart)
        {
            Validation.ThrowIfNull(spreadPart, nameof(spreadPart));

            // if a fragment is already assigned we dont need to do anything
            // this can happen if a fragment was read before a spread was encountered
            // this doesn't happen with the default doc generator, but custom implemnetations may
            // change that
            if (spreadPart.Fragment != null)
                return;

            // the document is not yet validated there might be multiple fragments
            // with the same name (yes its an error, but its not a "named fragment not referenced" error).
            // Assign any overflow fragments as being spread by this spreadPart
            if (_overflowFragments != null && _overflowFragments.ContainsKey(spreadPart.FragmentName))
            {
                foreach (var overflowNamedFragment in _overflowFragments[spreadPart.FragmentName])
                    overflowNamedFragment.SpreadBy(spreadPart);
            }

            // assign the "officially chosen" named fragment (the first encountered)
            // as the fragment referenced by the spread
            if (_fragments.TryGetValue(spreadPart.FragmentName, out var foundFragment))
            {
                spreadPart.AssignNamedFragment(foundFragment);
                foundFragment.SpreadBy(spreadPart);
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