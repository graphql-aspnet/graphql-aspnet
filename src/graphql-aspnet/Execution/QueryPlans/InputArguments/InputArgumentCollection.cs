// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.InputArguments
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

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

        /// <inheritdoc />
        public void Add(InputArgument input)
        {
            Validation.ThrowIfNull(input, nameof(input));
            _arguments.Add(input.Name, input);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IEnumerator<InputArgument> GetEnumerator()
        {
            return _arguments.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public bool Contains(string name)
        {
            return _arguments.ContainsKey(name);
        }

        /// <inheritdoc />
        public bool TryGetValue(string name, out InputArgument value)
        {
            return _arguments.TryGetValue(name, out value);
        }

        /// <inheritdoc />
        public InputArgument this[string name] => _arguments[name];

        /// <inheritdoc />
        public int Count => _arguments.Count;
    }
}