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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void SchemaSubscriptionRouteRegistered_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = new Mock<IGraphEventLogger>();
            mock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<IGraphLogEntry>>()))
                .Callback((LogLevel logLevel, Func<IGraphLogEntry> entryMaker) =>
                {
                    recordedLogLevel = logLevel;
                    recordedlogEntry = entryMaker();
                });

            mock.Object.SchemaSubscriptionRouteRegistered<GraphSchema>("testPath");

            var entry = recordedlogEntry as SchemaSubscriptionRouteRegisteredLogEntry<GraphSchema>;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(entry.SchemaTypeName, typeof(GraphSchema).FriendlyName(true));
            Assert.AreEqual(entry.SchemaSubscriptionRoutePath, "testPath");
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionClientRegistered_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = new Mock<IGraphEventLogger>();
            mock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<IGraphLogEntry>>()))
                .Callback((LogLevel logLevel, Func<IGraphLogEntry> entryMaker) =>
                {
                    recordedLogLevel = logLevel;
                    recordedlogEntry = entryMaker();
                });

            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();

            var id = SubscriptionClientId.NewClientId();
            client.Setup(x => x.Id).Returns(id);

            mock.Object.SubscriptionClientRegistered<GraphSchema>(client.Object);

            var entry = recordedlogEntry as SubscriptionClientRegisteredLogEntry<GraphSchema>;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(client.Object.GetType().FriendlyName(true), entry.ClientTypeName);
            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionClientDropped_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = new Mock<IGraphEventLogger>();
            mock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<IGraphLogEntry>>()))
                .Callback((LogLevel logLevel, Func<IGraphLogEntry> entryMaker) =>
                {
                    recordedLogLevel = logLevel;
                    recordedlogEntry = entryMaker();
                });

            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();

            var id = SubscriptionClientId.NewClientId();
            client.Setup(x => x.Id).Returns(id);

            mock.Object.SubscriptionClientDropped(client.Object);

            var entry = recordedlogEntry as SubscriptionClientDroppedLogEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(client.Object.GetType().FriendlyName(true), entry.ClientTypeName);
            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionEventReceived_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = new Mock<ILogger>();
            mock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<SubscriptionEventReceivedLogEntry>(),
                null,
                It.IsAny<Func<SubscriptionEventReceivedLogEntry, Exception, string>>()))
                .Callback((
                    LogLevel logLevel,
                    EventId eventId,
                    SubscriptionEventReceivedLogEntry state,
                    Exception ex,
                    Func<SubscriptionEventReceivedLogEntry, Exception, string> entryMaker) =>
                {
                    recordedLogLevel = logLevel;
                    recordedlogEntry = state;
                });

            mock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);

            var eventData = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                SchemaTypeName = "someSchemaName",
                Data = new object(),
                DataTypeName = "someTypeName",
                EventName = "testEvent",
            };

            mock.Object.SubscriptionEventReceived(eventData);

            var entry = recordedlogEntry as SubscriptionEventReceivedLogEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(entry.SubscriptionEventName, eventData.EventName);
            Assert.AreEqual(entry.DataType, eventData.DataTypeName);
            Assert.AreEqual(entry.SchemaType, eventData.SchemaTypeName);
            Assert.AreEqual(entry.SubscriptionEventId, eventData.Id);
            Assert.AreEqual(entry.MachineName, Environment.MachineName);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionEventPublished_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = new Mock<IGraphEventLogger>();
            mock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<IGraphLogEntry>>()))
                .Callback((LogLevel logLevel, Func<IGraphLogEntry> entryMaker) =>
                {
                    recordedLogLevel = logLevel;
                    recordedlogEntry = entryMaker();
                });

            var eventData = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                SchemaTypeName = "someSchemaName",
                Data = new object(),
                DataTypeName = "someTypeName",
                EventName = "testEvent",
            };

            mock.Object.SubscriptionEventPublished(eventData);

            var entry = recordedlogEntry as SubscriptionEventPublishedLogEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(entry.SubscriptionEventName, eventData.EventName);
            Assert.AreEqual(entry.DataType, eventData.DataTypeName);
            Assert.AreEqual(entry.SchemaType, eventData.SchemaTypeName);
            Assert.AreEqual(entry.SubscriptionEventId, eventData.Id);
            Assert.AreEqual(entry.MachineName, Environment.MachineName);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }
    }
}