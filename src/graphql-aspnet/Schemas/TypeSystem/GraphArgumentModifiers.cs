// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;

    /// <summary>
    /// A set of modifiers and flags that can be assigned to individual arguments on graph fields to modify their behavior
    /// during execution.
    /// </summary>
    [Flags]
    public enum GraphArgumentModifiers
    {
        /// <summary>
        /// No special modifications are needed.
        /// </summary>
        None = 0,

        /// <summary>
        /// This parameter is internal to the server environment and will not be exposed on the object graph.
        /// </summary>
        Internal = 1,

        /// <summary>
        /// This parameter is declared to contain the result of the resolved parent field.
        /// </summary>
        ParentFieldResult = 2,
    }
}