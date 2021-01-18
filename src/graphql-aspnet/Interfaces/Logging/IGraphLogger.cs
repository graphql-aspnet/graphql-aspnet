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
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A wrapper on <see cref="ILogger"/> providing a common format for all graphql logging capabilities.
    /// </summary>
    public interface IGraphLogger : ILogger
    {
        /// <summary>
        /// Writes the provided entry to the log if the log level is in scope for this instance.
        /// </summary>
        /// <param name="logLevel">The log level to record the entry as.</param>
        /// <param name="logEntry">The log entry to record.</param>
        void Log(LogLevel logLevel, IGraphLogEntry logEntry);

        /// <summary>
        /// When the provided log level is in scope and "loggable" the entry defined
        /// by <paramref name="entryMaker"/> will be created and logged.
        /// </summary>
        /// <param name="logLevel">The log level to write.</param>
        /// <param name="entryMaker">The function to generate the log entry, if needed.</param>
        void Log(LogLevel logLevel, Func<IGraphLogEntry> entryMaker);
    }
}