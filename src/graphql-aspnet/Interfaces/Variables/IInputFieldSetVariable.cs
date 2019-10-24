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
    /// A variable defined as a set of child key/value pair.
    /// </summary>
    public interface IInputFieldSetVariable : IInputVariable
    {
        /// <summary>
        /// Gets the dictionary of fields defined for this field set variable.
        /// </summary>
        /// <value>The fields.</value>
        IReadOnlyDictionary<string, IInputVariable> Fields { get; }
    }
}