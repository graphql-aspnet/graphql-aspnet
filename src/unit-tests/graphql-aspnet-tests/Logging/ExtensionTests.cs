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
    using Microsoft.Extensions.Logging;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Logging;
    using NUnit.Framework;
    using NSubstitute;
    using GraphQL.AspNet.Logging.GeneralEvents;

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
            var mock = Substitute.For<IGraphLogger>();
            mock.When(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<IGraphLogEntry>()))
                .Do(x =>
                {
                    recordedLogLevel = (LogLevel)x[0];
                    recordedlogEntry = (IGraphLogEntry)x[1];
                });

            var factory = Substitute.For<ILoggerFactory>();
            factory.CreateLogger(Arg.Any<string>()).Returns(null as ILogger);

            mock.UnhandledExceptionEvent(exception);

            var entry = recordedlogEntry as UnhandledExceptionLogEntry;
            Assert.AreEqual(LogLevel.Critical, recordedLogLevel);
            Assert.AreEqual(exception.Message, entry.ExceptionMessage);
            Assert.AreEqual(exception.StackTrace, entry.StackTrace);
            Assert.AreEqual(exception.GetType().FriendlyName(true), entry.TypeName);
        }
    }
}