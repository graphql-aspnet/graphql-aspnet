// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting
{
    using System;
    using System.Data;

    /// <summary>
    /// A bitwise set of format strategies that can be applied out of the box.
    /// </summary>
    [Flags]
    public enum NullabilityFormatStrategy
    {
        /// <summary>
        /// No changes are made to the nullability of different fields. They are used as provided in
        /// the source code. All strings and reference types passed to an argument or returned
        /// from a field are considered nullable unless otherwise overriden.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Intermediate graph types, created when you use path templates on your
        /// queries and mutations, are marked as non-nullable. This can greatly assist
        /// in reducing null-handling noise in various client side code generators.
        /// </summary>
        NonNullTemplates = 1,

        /// <summary>
        /// All lists, in any type expression, be that as input arguments
        /// or fields are treated as non-nullable by default.
        /// </summary>
        NonNullLists = 2,

        /// <summary>
        /// String scalars are treated like value types and cannot be null by default.
        /// </summary>
        NonNullStrings = 4,

        /// <summary>
        /// The schema will treat all class reference types, in any type expression, as being non-nullable by
        /// default.
        /// </summary>
        NonNullReferenceTypes = 8,
    }
}