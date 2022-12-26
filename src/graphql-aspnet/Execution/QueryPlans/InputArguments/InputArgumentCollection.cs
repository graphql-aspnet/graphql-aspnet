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
    /// A collection of arguments to be passed to a field or directive to assist in resolving it
    /// and generating a response.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    internal class InputArgumentCollection : IInputArgumentCollection
    {
        private readonly Dictionary<string, InputArgument> _arguments;

        // keep a list of arguments for when we need to enumerate between them
        // enumerator on dictionary values is 3x slower than a list's enumerator
        private readonly List<InputArgument> _argumentList;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgumentCollection"/> class.
        /// </summary>
        public InputArgumentCollection()
        {
            _arguments = new Dictionary<string, InputArgument>();
            _argumentList = new List<InputArgument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgumentCollection" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of arguments this collection will contain.</param>
        public InputArgumentCollection(int capacity)
        {
            _arguments = new Dictionary<string, InputArgument>(capacity);
            _argumentList = new List<InputArgument>(capacity);
        }

        /// <inheritdoc />
        public void Add(InputArgument input)
        {
            Validation.ThrowIfNull(input, nameof(input));
            _arguments.Add(input.Name, input);
            _argumentList.Add(input);
        }

        /// <inheritdoc />
        public IExecutionArgumentCollection Merge(IResolvedVariableCollection variableData)
        {
            var collection = new ExecutionArgumentCollection(_arguments.Count);
            foreach (var arg in _arguments.Values)
            {
                var resolvedValue = arg.Value.Resolve(variableData);
                collection.Add(new ExecutionArgument(arg.Argument, resolvedValue));
            }

            return collection;
        }

        /// <inheritdoc />
        public bool Contains(string name)
        {
            return _arguments.ContainsKey(name);
        }

        /// <inheritdoc />
        public IEnumerator<InputArgument> GetEnumerator()
        {
            return _argumentList.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public InputArgument this[string name] => _arguments[name];

        /// <inheritdoc />
        public int Count => _argumentList.Count;

        /// <inheritdoc />
        public InputArgument this[int index] => _argumentList[index];
    }
}