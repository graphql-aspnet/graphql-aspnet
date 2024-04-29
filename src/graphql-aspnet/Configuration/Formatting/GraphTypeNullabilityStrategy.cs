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
        None = 0,

        /// <summary>
        /// Intermediate graph types, created when you use path templates on your
        /// queries and mutations, are marked as non-nullable. This can greatly assist
        /// in reducing null-handling noise in various client side code generators. Intermediate graph
        /// types will never be null but may be expressed as "nullable" in the schema if this is not set.
        /// </summary>
        NonNullTemplates = 1,

        /// <summary>
        /// All lists, in any type expression, be that as input arguments
        /// or fields are treated as non-nullable by default.
        /// </summary>
        NonNullLists = 2,

        /// <summary>
        /// String scalars on input items (i.e. field arguments and INPUT_OBJECT fields) are treated like
        /// value types and considered "not nullable" by default.
        /// </summary>
        NonNullInputStrings = 4,

        /// <summary>
        /// String scalars on output items (i.e. fields on OBJECT and INTERFACE types) are treated like
        /// value types and considered "not nullable" by default.
        /// </summary>
        NonNullOutputStrings = 8,

        /// <summary>
        /// The schema will treat all class reference types, in any type expression, as being non-nullable by
        /// default.
        /// </summary>
        NonNullReferenceTypes = 16,

        /// <summary>
        /// All string scalars, whether as a field argument, input object field, object field or interface field
        /// are treated like value types and are considered "not nullable" by default in all instances.
        /// </summary>
        NonNullStrings = NonNullInputStrings | NonNullOutputStrings,

        /// <summary>
        /// No changes are made to the nullability of different fields. They are used as provided in
        /// the source code. All strings and reference types passed to an argument or returned
        /// from a field are considered nullable unless otherwise overriden.
        /// </summary>
        Default = NonNullTemplates,
    }
}