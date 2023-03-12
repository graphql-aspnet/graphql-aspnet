// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.Variables
{
    using System.Collections.Generic;

    /// <summary>
    /// A variable that represents a set/list of items supplied as a collection or an array by the user.
    /// </summary>
    public interface IWritableInputListVariable : IInputListVariable
    {
        /// <summary>
        /// Replaces the the input variable at the specified index with the given value.
        /// </summary>
        /// <param name="index">The index of the value to replace.</param>
        /// <param name="newValue">The new value.</param>
        void Replace(int index, IInputVariable newValue);
    }
}