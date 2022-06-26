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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of input arguments defined in a user's query document for a single field or directive.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentInputArgumentCollectionDocumentPart : IInputArgumentCollectionDocumentPart
    {
        private readonly IReadOnlyDictionary<string, IInputArgumentDocumentPart> _arguments;
        private IDocumentPart _ownerPart;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgumentCollectionDocumentPart" /> class.
        /// </summary>
        /// <param name="ownerPart">The document part which contains/owns all
        /// the collected input arguments.</param>
        public DocumentInputArgumentCollectionDocumentPart(IDocumentPart ownerPart)
        {
            _ownerPart = Validation.ThrowIfNullOrReturn(ownerPart, nameof(ownerPart));

            _arguments = _ownerPart.Children[DocumentPartType.Argument]
                .OfType<IInputArgumentDocumentPart>()
                .ToDictionary(x => x.Name);
        }

        /// <inheritdoc />
        public IInputArgumentDocumentPart FindArgumentByName(ReadOnlyMemory<char> name)
        {
            return this.FindArgumentByName(name.ToString());
        }

        /// <inheritdoc />
        public IInputArgumentDocumentPart FindArgumentByName(string name)
        {
            if (this.ContainsKey(name))
                return this[name];

            return null;
        }

        /// <inheritdoc />
        public bool ContainsKey(ReadOnlyMemory<char> inputName)
        {
            return this.ContainsKey(inputName.ToString());
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _arguments.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IInputArgumentDocumentPart value)
        {
            return _arguments.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IInputArgumentDocumentPart>> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _arguments.Count;

        /// <inheritdoc />
        public IInputArgumentDocumentPart this[string key] => _arguments[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _arguments.Keys;

        /// <inheritdoc />
        public IEnumerable<IInputArgumentDocumentPart> Values => _arguments.Values;
    }
}