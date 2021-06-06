// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A collection of variables that have been resolved and contextualized in terms
    /// of a specific query operation they are targeting.
    /// </summary>
    public class ResolvedVariableCollection : IResolvedVariableCollection
    {
        /// <summary>
        /// Gets a <see cref="IResolvedVariableCollection"/> containing no resolved items.
        /// </summary>
        /// <value>The empty.</value>
        public static IResolvedVariableCollection Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="ResolvedVariableCollection"/> class.
        /// </summary>
        static ResolvedVariableCollection()
        {
            Empty = new ResolvedVariableCollection();
        }

        private readonly Dictionary<string, IResolvedVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedVariableCollection"/> class.
        /// </summary>
        public ResolvedVariableCollection()
        {
            _variables = new Dictionary<string, IResolvedVariable>();
        }

        /// <summary>
        /// Adds the variable to the collection.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void AddVariable(IResolvedVariable variable)
        {
            _variables.Add(variable.Name, variable);
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string key)
        {
            return _variables.ContainsKey(key);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryGetValue(string key, out IResolvedVariable value)
        {
            return _variables.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the <see cref="IResolvedVariable"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>IResolvedVariable.</returns>
        public IResolvedVariable this[string key] => _variables[key];

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _variables.Keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<IResolvedVariable> Values => _variables.Values;

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _variables.Count;

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>IEnumerator&lt;KeyValuePair&lt;System.String, IResolvedVariable&gt;&gt;.</returns>
        public IEnumerator<KeyValuePair<string, IResolvedVariable>> GetEnumerator()
        {
            return _variables.GetEnumerator();
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