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
    /// A collection of variable references for a single operation that are used to validate any variable
    /// usage within an operation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class QueryVariableCollection : IQueryVariableCollection, IDocumentPart
    {
        private readonly Dictionary<string, QueryVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVariableCollection"/> class.
        /// </summary>
        public QueryVariableCollection()
        {
            _variables = new Dictionary<string, QueryVariable>();
        }

        /// <summary>
        /// Adds a parsed variable to this collection.
        /// </summary>
        /// <param name="variable">The variable to add.</param>
        public void AddVariable(QueryVariable variable)
        {
            Validation.ThrowIfNull(variable, nameof(variable));
            _variables.Add(variable.Name, variable);
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _variables.Count;

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return _variables.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out QueryVariable value)
        {
            return _variables.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, QueryVariable>> GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="QueryVariable"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>QueryVariable.</returns>
        public QueryVariable this[string key] => _variables[key];

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _variables.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<QueryVariable> Values => _variables.Values;

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                return _variables.Values;
            }
        }
    }
}