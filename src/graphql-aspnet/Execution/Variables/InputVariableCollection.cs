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
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Variables.Json;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A collection of variables supplied by user to be used when resolving
    /// a query operation.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [JsonConverter(typeof(InputVariableCollectionConverter))]
    public partial class InputVariableCollection : IInputVariableCollection, IWritableInputVariableCollection
    {
        private readonly Dictionary<string, IInputVariable> _variableSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputVariableCollection"/> class.
        /// </summary>
        public InputVariableCollection()
        {
            _variableSet = new Dictionary<string, IInputVariable>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputVariableCollection" /> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection to initialize from.</param>
        public InputVariableCollection(IInputVariableCollection sourceCollection)
        {
            _variableSet = new Dictionary<string, IInputVariable>();

            if (sourceCollection != null)
            {
                foreach (var variable in sourceCollection)
                    _variableSet.Add(variable.Key, variable.Value);
            }
        }

        /// <summary>
        /// Adds the specified variable to the collection.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void Add(IInputVariable variable)
        {
            _variableSet.Add(variable.Name, variable);
        }

        /// <inheritdoc />
        public bool TryGetVariable(string name, out IInputVariable variable)
        {
            return _variableSet.TryGetValue(name, out variable);
        }

        /// <inheritdoc />
        public void Replace(string variableName, IInputVariable newValue)
        {
            Validation.ThrowIfNull(variableName, nameof(variableName));
            Validation.ThrowIfNull(newValue, nameof(newValue));

            if (!_variableSet.ContainsKey(variableName))
                throw new KeyNotFoundException(variableName);

            _variableSet[variableName] = newValue;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IInputVariable>> GetEnumerator()
        {
            return _variableSet.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _variableSet.Count;
    }
}