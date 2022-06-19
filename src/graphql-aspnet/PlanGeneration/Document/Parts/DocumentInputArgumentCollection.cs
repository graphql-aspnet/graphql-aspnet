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
    internal class DocumentInputArgumentCollection : DocumentPartBase<IInputArgumentCollectionDocumentPart>, IInputArgumentCollectionDocumentPart
    {
        private readonly Dictionary<string, IInputArgumentDocumentPart> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgumentCollection" /> class.
        /// </summary>
        /// <param name="parentContainer">The parent container that owns this collection instance.</param>
        public DocumentInputArgumentCollection(IInputArgumentContainerDocumentPart parentContainer)
            : base(parentContainer)
        {
            _arguments = new Dictionary<string, IInputArgumentDocumentPart>();
        }

        /// <inheritdoc />
        public void AddArgument(IInputArgumentDocumentPart argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _arguments.Add(argument.Name, argument);
            argument.AssignParent(this);
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

        /// <inheritdoc />
        public override IEnumerable<IDocumentPart> Children => _arguments.Values;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.InputArgumentCollection;
    }
}