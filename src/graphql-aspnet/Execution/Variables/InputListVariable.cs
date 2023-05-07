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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A variable that represents a set/list of items supplied as a collection or an array by the user.
    /// </summary>
    [DebuggerDisplay("InputList: {Name}, Count = {Items.Count}")]
    internal class InputListVariable : InputVariable, IInputListVariable, IWritableInputListVariable, IResolvableList
    {
        private readonly List<IInputVariable> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputListVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        public InputListVariable(string name)
            : base(name)
        {
            _items = new List<IInputVariable>();
        }

        /// <inheritdoc />
        public void Replace(int index, IInputVariable newValue)
        {
            if (index < 0 || _items.Count <= index)
                throw new IndexOutOfRangeException($"Invalid index ({index}). New variable value cannot be set.");

            _items[index] = newValue;
        }

        /// <summary>
        /// Adds the variable as a child of this list variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void AddVariable(IInputVariable variable)
        {
            _items.Add(variable);
        }

        /// <inheritdoc />
        public IEnumerator<IResolvableKeyedItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IReadOnlyList<IInputVariable> Items => _items;
    }
}