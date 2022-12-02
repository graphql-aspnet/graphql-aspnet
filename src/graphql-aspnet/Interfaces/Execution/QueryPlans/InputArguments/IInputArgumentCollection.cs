// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A collection of <see cref="IInputValue"/> to be used to invoke a method or action field type on the graph. Keyed
    /// on internal name representations.
    /// </summary>
    public interface IInputArgumentCollection : IReadOnlyList<InputArgument>
    {
        /// <summary>
        /// Adds the specified argument to the collection.
        /// </summary>
        /// <param name="input">The input.</param>
        void Add(InputArgument input);

        /// <summary>
        /// Merges the supplied variable data into a new collection of arguments
        /// capable of fulfilling a request. Any deferred arguments in this instance are resolved
        /// to the data contained in the provided dataset or null if not found.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputArgumentCollection.</returns>
        IExecutionArgumentCollection Merge(IResolvedVariableCollection variableData);

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="name">The name of the variable to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        bool Contains(string name);

        /// <summary>
        /// Gets the <see cref="InputArgument"/> with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>InputArgument.</returns>
        InputArgument this[string name] { get; }
    }
}