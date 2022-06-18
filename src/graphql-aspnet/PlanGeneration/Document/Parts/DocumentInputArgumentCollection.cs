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

    /// <summary>
    /// A collection of input arguments defined in a user's query document for a single field or directive.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class DocumentInputArgumentCollection : IQueryInputArgumentCollectionDocumentPart
    {
        private readonly Dictionary<string, IQueryArgumentDocumentPart> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgumentCollection" /> class.
        /// </summary>
        public DocumentInputArgumentCollection()
        {
            _arguments = new Dictionary<string, IQueryArgumentDocumentPart>();
        }

        /// <inheritdoc />
        public void AddArgument(IQueryArgumentDocumentPart argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _arguments.Add(argument.Name, argument);
        }

        /// <inheritdoc />
        public IQueryArgumentDocumentPart FindArgumentByName(ReadOnlyMemory<char> name)
        {
            return this.FindArgumentByName(name.ToString());
        }

        /// <inheritdoc />
        public IQueryArgumentDocumentPart FindArgumentByName(string name)
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
        public bool TryGetValue(string key, out IQueryArgumentDocumentPart value)
        {
            return _arguments.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IQueryArgumentDocumentPart>> GetEnumerator()
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
        public IQueryArgumentDocumentPart this[string key] => _arguments[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _arguments.Keys;

        /// <inheritdoc />
        public IEnumerable<IQueryArgumentDocumentPart> Values => _arguments.Values;

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children => _arguments.Values;

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.InputArgumentCollection;
    }
}