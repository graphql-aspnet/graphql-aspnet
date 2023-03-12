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
    /// A variable defined as a set of child key/value pair.
    /// </summary>
    public interface IWritableInputFieldSetVariable: IInputFieldSetVariable
    {
        /// <summary>
        /// Replaces the the input variable of the specified name with the new given value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="newValue">The new value.</param>
        void Replace(string fieldName, IInputVariable newValue);
    }
}