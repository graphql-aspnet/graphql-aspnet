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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of fragments parsed from the document that may be referenced by the various operations in the document.
    /// </summary>
    internal class DocumentOperationCollection : DocumentPartBase, IOperationCollectionDocumentPart
    {
        private readonly Dictionary<string, IOperationDocumentPart> _operations;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentOperationCollection"/> class.
        /// </summary>
        public DocumentOperationCollection()
        {
            _operations = new Dictionary<string, IOperationDocumentPart>();
        }

        /// <inheritdoc/>
        public void AddOperation(IOperationDocumentPart operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));
            _operations.Add(operation.Name, operation);
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<IOperationDocumentPart> operations)
        {
            if (operations != null)
            {
                foreach (var operation in operations)
                    this.AddOperation(operation);
            }
        }

        /// <inheritdoc/>
        public int Count => _operations.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return _operations.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out IOperationDocumentPart value)
        {
            return _operations.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public IOperationDocumentPart this[string key] => _operations[key];

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _operations.Keys;

        /// <inheritdoc/>
        public IEnumerable<IOperationDocumentPart> Values => _operations.Values;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, IOperationDocumentPart>> GetEnumerator()
        {
            return _operations.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.OperationCollection;

        /// <inheritdoc />
        public override IEnumerable<IDocumentPart> Children => _operations.Values;
    }
}