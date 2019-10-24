// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging
{
    using System;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Logging.ExecutionEvents;
    using Microsoft.Extensions.Logging;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Logging.Extensions;
    using NUnit.Framework;
    using Moq;

    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void UnHandledException_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            Exception exception = null;
            try
            {
                throw new InvalidCastException("test failure");
            }
            catch (Exception ex)
            {
                // generate an exception witha  stack trace
                exception = ex;
            }

            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = new Mock<IGraphLogger>();
            mock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<IGraphLogEntry>()))
                .Callback((LogLevel logLevel, IGraphLogEntry logEntry) =>
                    {
                        recordedLogLevel = logLevel;
                        recordedlogEntry = logEntry;
                    });

            var factory = new Mock<ILoggerFactory>();
            factory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(null as ILogger);

            mock.Object.UnhandledExceptionEvent(exception);

            var entry = recordedlogEntry as UnhandledExceptionLogEntry;
            Assert.AreEqual(LogLevel.Critical, recordedLogLevel);
            Assert.AreEqual(exception.Message, entry.ExceptionMessage);
            Assert.AreEqual(exception.StackTrace, entry.StackTrace);
            Assert.AreEqual(exception.GetType().FriendlyName(true), entry.TypeName);
        }
    }
}