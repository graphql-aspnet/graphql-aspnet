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
    /// A variable that represents a set/list of items supplied as a collection or an array by the user.
    /// </summary>
    public interface IInputListVariable : IInputVariable
    {
        /// <summary>
        /// Gets the collection of items contained in this list variable.
        /// </summary>
        /// <value>The items.</value>
        IReadOnlyList<IInputVariable> Items { get; }
    }
}