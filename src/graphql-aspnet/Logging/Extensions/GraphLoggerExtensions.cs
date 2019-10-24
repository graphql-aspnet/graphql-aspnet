// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.Extensions
{
    using System;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Logging.ExecutionEvents;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Helper methods related to <see cref="IGraphLogger"/>.
    /// </summary>
    public static class GraphLoggerExtensions
    {
        /// <summary>
        /// Creates a log entry a set of common information describing the unhandled exception.
        /// </summary>
        /// <param name="logger">The logger to write the log entry to.</param>
        /// <param name="exception">The exception that was unhandled.</param>
        /// <param name="logLevel">The log level to apply to the entry when its created.</param>
        public static void UnhandledExceptionEvent(this IGraphLogger logger, Exception exception, LogLevel logLevel = LogLevel.Critical)
        {
            var entry = new UnhandledExceptionLogEntry(exception);
            logger.Log(logLevel, entry);
        }
    }
}