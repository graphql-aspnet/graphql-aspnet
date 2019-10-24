// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Logging
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A wrapper on <see cref="ILogger"/> providing a common format for all graphql logging capabilities.
    /// </summary>
    public interface IGraphLogger : ILogger
    {
        /// <summary>
        /// Writes the provided entry to the log.
        /// </summary>
        /// <param name="logLevel">The log level to record the entry as.</param>
        /// <param name="logEntry">The log entry to record.</param>
        void Log(LogLevel logLevel, IGraphLogEntry logEntry);
    }
}