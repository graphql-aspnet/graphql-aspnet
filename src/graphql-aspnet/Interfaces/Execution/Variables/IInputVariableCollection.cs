// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Variables
{
    using System.Collections.Generic;

    /// <summary>
    /// A collection of variables supplied by user to be used when resolving
    /// a query operation.
    /// </summary>
    public interface IInputVariableCollection : IEnumerable<KeyValuePair<string, IInputVariable>>
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="name">The name of the value to get.</param>
        /// <param name="variable"> When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns> true if the instnace contains an element with the specified key; otherwise, false.</returns>
        bool TryGetVariable(string name, out IInputVariable variable);

        /// <summary>
        /// Gets the number of variables stored in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }
    }
}