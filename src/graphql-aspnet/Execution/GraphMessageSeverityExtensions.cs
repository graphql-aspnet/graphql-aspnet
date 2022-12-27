// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    /// <summary>
    /// Extension methods related to the <see cref="GraphMessageSeverity"/> enumeration.
    /// </summary>
    public static class GraphMessageSeverityExtensions
    {
        /// <summary>
        /// Determines whether the specified severity is of a critical level (vs just informational).
        /// </summary>
        /// <param name="severity">The severity to inspect.</param>
        /// <returns><c>true</c> if the specified severity is critical; otherwise, <c>false</c>.</returns>
        public static bool IsCritical(this GraphMessageSeverity severity)
        {
            return severity >= GraphMessageSeverity.Critical;
        }
    }
}