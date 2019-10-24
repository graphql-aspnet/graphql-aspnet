// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.PlanGeneration.InputArguments;

    /// <summary>
    /// A collection of <see cref="IInputArgumentValue"/> to be used to invoke a method or action field type on the graph. Keyed
    /// on internal name representations.
    /// </summary>
    public interface IInputArgumentCollection : IEnumerable<InputArgument>
    {
        /// <summary>
        /// Adds the specified argument to the collection.
        /// </summary>
        /// <param name="input">The input.</param>
        void Add(InputArgument input);

        /// <summary>
        /// Merges the supplied variable data into a colleciton a new collection of arguments
        /// capable of fulfilling a request. Any deferred arguments in this instance are resolved
        /// to the data contained in the provided dataset or null if not found.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputArgumentCollection.</returns>
        IExecutionArgumentCollection Merge(IResolvedVariableCollection variableData);

        /// <summary>
        /// Gets the count of arguments in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="name">The name of the variable to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        bool Contains(string name);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="name">The name of the variable to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        bool TryGetValue(string name, out InputArgument value);

        /// <summary>
        /// Gets the <see cref="InputArgument"/> with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>InputArgument.</returns>
        InputArgument this[string name] { get; }
    }
}