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
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Json;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A collection of variables supplied by user to be used when resolving
    /// a query operation.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [JsonConverter(typeof(InputVariableCollectionConverter))]
    public class InputVariableCollection : IInputVariableCollection
    {
        /// <summary>
        /// Creates a qualified <see cref="InputVariableCollection"/> from a given json string.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <returns>IInputVariableCollection.</returns>
        public static InputVariableCollection FromJsonDocument(string jsonDocument)
        {
            var options = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            return JsonSerializer.Deserialize<InputVariableCollection>(jsonDocument, options);
        }

        /// <summary>
        /// Gets a singleton instance of an empty variable collection.
        /// </summary>
        /// <value>The empty.</value>
        public static InputVariableCollection Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="InputVariableCollection"/> class.
        /// </summary>
        static InputVariableCollection()
        {
            Empty = new InputVariableCollection();
        }

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

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="name">The name of the value to get.</param>
        /// <param name="variable">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the instnace contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetVariable(string name, out IInputVariable variable)
        {
            return _variableSet.TryGetValue(name, out variable);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, IInputVariable>> GetEnumerator()
        {
            return _variableSet.GetEnumerator();
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
        /// Gets the number of variables stored in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _variableSet.Count;
    }
}