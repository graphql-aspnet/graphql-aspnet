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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// The collection of operations parsed and defined on a query document.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentOperationCollection : IOperationCollectionDocumentPart
    {
        private readonly Dictionary<string, IOperationDocumentPart> _operations;
        private readonly List<IOperationDocumentPart> _operationsInOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentOperationCollection"/> class.
        /// </summary>
        /// <param name="owner">The query document that owns this set of operations.</param>
        public DocumentOperationCollection(IGraphQueryDocument owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _operations = new Dictionary<string, IOperationDocumentPart>();
            _operationsInOrder = new List<IOperationDocumentPart>();
        }

        /// <summary>
        /// Adds the operation to this collection.
        /// </summary>
        /// <param name="operation">The operation to add.</param>
        public void AddOperation(IOperationDocumentPart operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));

            var name = operation.Name;
            if (name == null)
                name = string.Empty;

            name = name.Trim();
            if (!_operations.ContainsKey(name))
                _operations.Add(name, operation);

            _operationsInOrder.Add(operation);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _operations.ContainsKey(key);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IOperationDocumentPart>> GetEnumerator()
        {
            return _operations.GetEnumerator();
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IOperationDocumentPart value)
        {
            return _operations.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IOperationDocumentPart RetrieveOperation(string operationName = null)
        {
            operationName = operationName?.Trim() ?? string.Empty;
            if (this.TryGetValue(operationName, out var operation))
                return operation;

            return null;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IOperationDocumentPart this[string key] => _operations[key];

        /// <inheritdoc />
        public IGraphQueryDocument Owner { get; }

        /// <inheritdoc />
        public IEnumerable<string> Keys => _operations.Keys;

        /// <inheritdoc />
        public IEnumerable<IOperationDocumentPart> Values => _operations.Values;

        /// <inheritdoc />
        public int Count => _operations.Count;

        /// <inheritdoc />
        public IOperationDocumentPart this[int index] => _operationsInOrder[index];
    }
}