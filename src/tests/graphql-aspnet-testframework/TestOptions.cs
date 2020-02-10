// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System;

    /// <summary>
    /// A set of common test configurations to use when setting up a test server.
    /// </summary>
    [Flags]
    public enum TestOptions
    {
        /// <summary>
        /// All settings are used with their default values, no changes are made.
        /// </summary>
        None = 0,

        /// <summary>
        /// Exceptions are exposed on all requests serialized to JSON.
        /// </summary>
        IncludeExceptions = 1,

        /// <summary>
        /// Metrics are automatically enabled and exposed on all requests.
        /// </summary>
        IncludeMetrics = 2,

        /// <summary>
        /// All Graph Types, Fields and Enum Values are registered to the schema as they are declared
        /// in the source code.  This enables the use of "nameof()" to avoid some magic strings in test code.
        /// </summary>
        UseCodeDeclaredNames = 4,
    }
}