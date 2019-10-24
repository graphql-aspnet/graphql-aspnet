// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.InputArguments
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A collection of arguments to be passed to a field to assist in resolving it and generating a response.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class InputArgumentCollection : IInputArgumentCollection
    {
        private readonly Dictionary<string, InputArgument> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgumentCollection"/> class.
        /// </summary>
        public InputArgumentCollection()
        {
            _arguments = new Dictionary<string, InputArgument>();
        }

        /// <summary>
        /// Adds the specified argument to the collection.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Add(InputArgument input)
        {
            Validation.ThrowIfNull(input, nameof(input));
            _arguments.Add(input.Name, input);
        }

        /// <summary>
        /// Merges the supplied variable data into a new collection of arguments
        /// capable of fulfilling a request. Any deferred arguments in this instance are resolved
        /// to the data contained in the provided set or null if not found.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputArgumentCollection.</returns>
        public IExecutionArgumentCollection Merge(IResolvedVariableCollection variableData)
        {
            var collection = new ExecutionArgumentCollection();
            foreach (var arg in _arguments.Values)
            {
                var resolvedValue = arg.Value.Resolve(variableData);
                collection.Add(new ExecutionArgument(arg.Argument, resolvedValue));
            }

            return collection;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<InputArgument> GetEnumerator()
        {
            return _arguments.Values.GetEnumerator();
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
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="name">The name of the variable to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        public bool Contains(string name)
        {
            return _arguments.ContainsKey(name);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="name">The name of the variable to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        public bool TryGetValue(string name, out InputArgument value)
        {
            return _arguments.TryGetValue(name, out value);
        }

        /// <summary>
        /// Gets the <see cref="InputArgument"/> with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>InputArgument.</returns>
        public InputArgument this[string name] => _arguments[name];

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _arguments.Count;
    }
}