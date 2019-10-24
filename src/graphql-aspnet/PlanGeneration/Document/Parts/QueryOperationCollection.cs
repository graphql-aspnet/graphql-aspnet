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
    public class QueryOperationCollection : IQueryOperationCollection
    {
        private readonly Dictionary<string, QueryOperation> _operations;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryOperationCollection"/> class.
        /// </summary>
        public QueryOperationCollection()
        {
            _operations = new Dictionary<string, QueryOperation>();
        }

        /// <summary>
        /// Adds the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        public void AddOperation(QueryOperation operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));
            _operations.Add(operation.Name, operation);
        }

        /// <summary>
        /// Adds the set of <see cref="QueryOperation"/> to this collection, keyed by the name
        /// provided in the user query document.
        /// </summary>
        /// <param name="operations">The set of operations to add.</param>
        public void AddRange(IEnumerable<QueryOperation> operations)
        {
            if (operations != null)
            {
                foreach (var operation in operations)
                    this.AddOperation(operation);
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _operations.Count;

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string key)
        {
            return _operations.ContainsKey(key);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryGetValue(string key, out QueryOperation value)
        {
            return _operations.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the <see cref="QueryOperation"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>QueryOperation.</returns>
        public QueryOperation this[string key] => _operations[key];

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _operations.Keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<QueryOperation> Values => _operations.Values;

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>IEnumerator&lt;KeyValuePair&lt;System.String, QueryOperation&gt;&gt;.</returns>
        public IEnumerator<KeyValuePair<string, QueryOperation>> GetEnumerator()
        {
            return _operations.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}