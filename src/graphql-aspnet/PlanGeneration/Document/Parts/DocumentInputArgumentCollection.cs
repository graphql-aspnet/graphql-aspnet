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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// A collection of input arguments defined in a user's query document for a single field or directive.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentInputArgumentCollection : IInputArgumentCollectionDocumentPart
    {
        private readonly Dictionary<string, IInputArgumentDocumentPart> _arguments;
        private IDocumentPart _ownerPart;
        private HashSet<string> _duplicateArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgumentCollection" /> class.
        /// </summary>
        /// <param name="ownerPart">The document part which contains/owns all
        /// the collected input arguments.</param>
        public DocumentInputArgumentCollection(IDocumentPart ownerPart)
        {
            _ownerPart = Validation.ThrowIfNullOrReturn(ownerPart, nameof(ownerPart));
            _arguments = new Dictionary<string, IInputArgumentDocumentPart>();
            _duplicateArguments = new HashSet<string>();
        }

        /// <summary>
        /// Adds the argumment to the collection.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void AddArgumment(IInputArgumentDocumentPart argument)
        {
            if (!_arguments.ContainsKey(argument.Name))
            {
                _arguments.Add(argument.Name, argument);
            }
            else
            {
                _duplicateArguments.Add(argument.Name);
            }
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
        public bool IsUnique(ReadOnlySpan<char> name)
        {
            return this.IsUnique(name.ToString());
        }

        /// <inheritdoc />
        public bool IsUnique(string name)
        {
            if (name == null)
                return false;

            return !_duplicateArguments.Contains(name);
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