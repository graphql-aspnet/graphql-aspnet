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
    /// A collection of variables supplied by user to be used when resolving
    /// a query operation.
    /// </summary>
    public interface IWritableInputVariableCollection : IInputVariableCollection
    {
        /// <summary>
        /// Replaces a variable with the specified name (case-sensitive) with a new variable value. The
        /// variable must already exist or an exception will be thrown.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="variable">The new variable value.</param>
        void Replace(string name, IInputVariable variable);
    }
}