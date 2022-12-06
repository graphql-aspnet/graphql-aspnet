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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A collection of input arguments defined in a user's query document for a single field or directive.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentInputObjectFieldCollection : IInputObjectFieldCollectionDocumentPart
    {
        private readonly Dictionary<string, IInputObjectFieldDocumentPart> _fields;
        private IDocumentPart _ownerPart;
        private HashSet<string> _duplicateFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputObjectFieldCollection" /> class.
        /// </summary>
        /// <param name="ownerPart">The document part which contains/owns all
        /// the collected input fields.</param>
        public DocumentInputObjectFieldCollection(IDocumentPart ownerPart)
        {
            _ownerPart = Validation.ThrowIfNullOrReturn(ownerPart, nameof(ownerPart));
            _fields = new Dictionary<string, IInputObjectFieldDocumentPart>();
            _duplicateFields = new HashSet<string>();
        }

        /// <summary>
        /// Adds the field to the collection.
        /// </summary>
        /// <param name="argument">The field to add.</param>
        public void AddField(IInputObjectFieldDocumentPart argument)
        {
            if (!_fields.ContainsKey(argument.Name))
            {
                _fields.Add(argument.Name, argument);
            }
            else
            {
                _duplicateFields.Add(argument.Name);
            }
        }

        /// <inheritdoc />
        public IInputObjectFieldDocumentPart FindFieldByName(ReadOnlyMemory<char> name)
        {
            return this.FindFieldByName(name.ToString());
        }

        /// <inheritdoc />
        public IInputObjectFieldDocumentPart FindFieldByName(string name)
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

            return !_duplicateFields.Contains(name);
        }

        /// <inheritdoc />
        public bool ContainsKey(ReadOnlyMemory<char> inputName)
        {
            return this.ContainsKey(inputName.ToString());
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IInputObjectFieldDocumentPart value)
        {
            return _fields.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IInputObjectFieldDocumentPart>> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _fields.Count;

        /// <inheritdoc />
        public IInputObjectFieldDocumentPart this[string key] => _fields[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _fields.Keys;

        /// <inheritdoc />
        public IEnumerable<IInputObjectFieldDocumentPart> Values => _fields.Values;
    }
}