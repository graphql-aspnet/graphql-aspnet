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
    /// A set of metadata options that can be applied to a field or input value to describe its
    /// allowed behaviors.
    /// </summary>
    [Flags]
    public enum TypeExpressions
    {
        /// <summary>
        /// Denotes an explicit declaration of NO modifiers. GraphQL will expect a single item or nothing at all.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes no special modifiers. GraphQL will attempt to generate a valid type expression from the data type.
        /// This is the default value.
        /// </summary>
        Auto = 1,

        /// <summary>
        /// Denotes that a value is expected and cannot be null. If the field
        /// returns a list, then this indicates the elements of the list must not be null.
        /// </summary>
        IsNotNull = 2,

        /// <summary>
        /// Denotes that a list of items or null is expected.
        /// </summary>
        IsList = 4,

        /// <summary>
        /// Denotes that a list of items is expected and cannot be null. The list of items may be empty.
        /// </summary>
        IsNotNullList = 8,
    }
}