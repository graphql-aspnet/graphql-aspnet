// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Logging.LoggerTestData
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Logging;
    using Microsoft.Extensions.Logging;

    public class TestLogger : ILogger
    {
        public TestLogger()
        {
            this.LogEntries = new List<IGraphLogEntry>();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (this.IsEnabled(logLevel))
            {
                if (state is IGraphLogEntry logEntry)
                {
                    this.LogEntries.Add(logEntry);
                }
            }
        }

        public IList<IGraphLogEntry> LogEntries { get; }
    }
}