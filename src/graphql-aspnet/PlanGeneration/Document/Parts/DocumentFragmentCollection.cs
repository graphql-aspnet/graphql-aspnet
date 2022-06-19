// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of fragments parsed from the document that may be referenced by the various operations in the document.
    /// </summary>
    internal class DocumentFragmentCollection : DocumentPartBase, IFragmentCollectionDocumentPart
    {
        private Dictionary<string, IFragmentDocumentPart> _fragments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentCollection"/> class.
        /// </summary>
        public DocumentFragmentCollection()
        {
            _fragments = new Dictionary<string, IFragmentDocumentPart>();
        }

        /// <inheritdoc />
        public IFragmentDocumentPart FindFragment(string name)
        {
            if (_fragments.ContainsKey(name))
                return _fragments[name];
            return null;
        }

        /// <inheritdoc />
        public void AddFragment(IFragmentDocumentPart fragment)
        {
            Validation.ThrowIfNull(fragment, nameof(fragment));
            _fragments.Add(fragment.Name, fragment);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key) => _fragments.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, out IFragmentDocumentPart value)
        {
            value = default;
            return _fragments.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IFragmentDocumentPart>> GetEnumerator()
        {
            return _fragments.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public override IEnumerable<IDocumentPart> Children => _fragments.Values;

        /// <inheritdoc />
        public IEnumerable<string> Keys => _fragments.Keys;

        /// <inheritdoc />
        public IEnumerable<IFragmentDocumentPart> Values => _fragments.Values;

        /// <inheritdoc />
        public int Count => _fragments.Count;

        /// <inheritdoc />
        public IFragmentDocumentPart this[string key] => _fragments[key];

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FragmentCollection;
    }
}