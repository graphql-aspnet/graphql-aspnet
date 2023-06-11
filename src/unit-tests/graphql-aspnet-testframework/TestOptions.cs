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
        /// All settings are initially configured with their
        /// "library default values", no special changes are made.
        /// </summary>
        None = 0,

        /// <summary>
        /// Exceptions are automatically exposed on all requests serialized to JSON strings.
        /// </summary>
        IncludeExceptions = 1,

        /// <summary>
        /// Metrics are automatically enabled and added on all requests. Metric results are
        /// automatically serialized to any JSON strings when created.
        /// </summary>
        IncludeMetrics = 2,

        /// <summary>
        /// All Graph Types, Fields and Enum Values are registered to the schema as they are declared
        /// in the source code.  This allows use easy use of '<c>nameof()</c>' to avoid some magic strings
        /// in test code.
        /// </summary>
        UseCodeDeclaredNames = 4,

        /// <summary>
        /// Causes introspection data not to be added to the schema. This can be useful when testing
        /// some negative scenarios to prevent premature failure due to introspection validation.
        /// </summary>
        SkipIntrospectionData = 8,
    }
}