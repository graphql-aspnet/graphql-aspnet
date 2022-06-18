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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of fragments parsed from the document that may be referenced by the various operations in the document.
    /// </summary>
    public class DocumentOperationCollection : IQueryOperationCollectionDocumentPart
    {
        private readonly Dictionary<string, IQueryOperationDocumentPart> _operations;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentOperationCollection"/> class.
        /// </summary>
        public DocumentOperationCollection()
        {
            _operations = new Dictionary<string, IQueryOperationDocumentPart>();
        }

        /// <inheritdoc/>
        public void AddOperation(IQueryOperationDocumentPart operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));
            _operations.Add(operation.Name, operation);
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<IQueryOperationDocumentPart> operations)
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
        public bool TryGetValue(string key, out IQueryOperationDocumentPart value)
        {
            return _operations.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public IQueryOperationDocumentPart this[string key] => _operations[key];

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _operations.Keys;

        /// <inheritdoc/>
        public IEnumerable<IQueryOperationDocumentPart> Values => _operations.Values;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, IQueryOperationDocumentPart>> GetEnumerator()
        {
            return _operations.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.OperationCollection;

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children => _operations.Values;
    }
}