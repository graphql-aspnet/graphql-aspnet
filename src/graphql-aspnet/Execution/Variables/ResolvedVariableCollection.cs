// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A collection of variables that have been resolved and contextualized in terms
    /// of a specific query operation they are targeting.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class ResolvedVariableCollection : IResolvedVariableCollection
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

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _variables.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IResolvedVariable value)
        {
            return _variables.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IResolvedVariable this[string key] => _variables[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _variables.Keys;

        /// <inheritdoc />
        public IEnumerable<IResolvedVariable> Values => _variables.Values;

        /// <inheritdoc />
        public int Count => _variables.Count;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IResolvedVariable>> GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}