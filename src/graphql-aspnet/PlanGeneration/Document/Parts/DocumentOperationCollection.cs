namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentOperationCollection : IOperationCollectionDocumentPart
    {
        private readonly Dictionary<string, IOperationDocumentPart> _operations;
        private List<IOperationDocumentPart> _operationsInOrder;

        public DocumentOperationCollection(IGraphQueryDocument owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _operations = new Dictionary<string, IOperationDocumentPart>();
            _operationsInOrder = new List<IOperationDocumentPart>();
        }

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

        public bool ContainsKey(string key)
        {
            return _operations.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, IOperationDocumentPart>> GetEnumerator()
        {
            return _operations.GetEnumerator();
        }

        public bool TryGetValue(string key, out IOperationDocumentPart value)
        {
            return _operations.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IOperationDocumentPart this[string key] => _operations[key];

        public IGraphQueryDocument Owner { get; }


        public IEnumerable<string> Keys => _operations.Keys;
        public IEnumerable<IOperationDocumentPart> Values => _operations.Values;
        public int Count => _operations.Count;

        public IOperationDocumentPart this[int index] => _operationsInOrder[index];
    }
}
