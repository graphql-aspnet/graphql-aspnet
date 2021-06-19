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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A variable that represents a set/list of items supplied as a collection or an array by the user.
    /// </summary>
    [DebuggerDisplay("InputList: {Name}, Count = {Items.Count}")]
    public class InputListVariable : InputVariable, IInputListVariable, IResolvableList
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

        /// <summary>
        /// Adds the variable as a child of this list variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void AddVariable(IInputVariable variable)
        {
            _items.Add(variable);
        }

        /// <summary>
        /// Gets the collection of items contained in this list variable.
        /// </summary>
        /// <value>The items.</value>
        public IReadOnlyList<IInputVariable> Items => _items;

        /// <summary>
        /// Gets the list of other resolvable items contained in this list.
        /// </summary>
        /// <value>The list items.</value>
        IEnumerable<IResolvableItem> IResolvableList.ListItems => this.Items;
    }
}