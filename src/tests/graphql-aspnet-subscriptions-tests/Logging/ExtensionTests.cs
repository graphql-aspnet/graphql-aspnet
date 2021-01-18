// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Logging
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.Schemas;
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

            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.Id).Returns("server1");

            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();
            client.Setup(x => x.Id).Returns("clientId1");

            mock.Object.SubscriptionClientRegistered<GraphSchema>(
                server.Object,
                client.Object);

            var entry = recordedlogEntry as SubscriptionClientRegisteredLogEntry<GraphSchema>;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(entry.SchemaTypeName, typeof(GraphSchema).FriendlyName(true));
            Assert.AreEqual(entry.ClientTypeName, client.Object.GetType().FriendlyName(true));
            Assert.AreEqual(entry.ServerTypeName, server.Object.GetType().FriendlyName(true));
            Assert.AreEqual(entry.ServerId, "server1");
            Assert.AreEqual(entry.ClientId, "clientId1");
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
            client.Setup(x => x.Id).Returns("clientId1");

            mock.Object.SubscriptionClientDropped(client.Object);

            var entry = recordedlogEntry as SubscriptionClientDroppedLogEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(entry.ClientTypeName, client.Object.GetType().FriendlyName(true));
            Assert.AreEqual(entry.ClientId, "clientId1");
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionServerCreated_EnsureEventEntryIsGeneratedAndPassedThrough()
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

            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.Id).Returns("server1");

            mock.Object.SubscriptionServerCreated<GraphSchema>(
                server.Object);

            var entry = recordedlogEntry as SubscriptionServerCreatedLogEntry<GraphSchema>;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(entry.SchemaTypeName, typeof(GraphSchema).FriendlyName(true));
            Assert.AreEqual(entry.ServerTypeName, server.Object.GetType().FriendlyName(true));
            Assert.AreEqual(entry.ServerId, "server1");
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionEventReceived_EnsureEventEntryIsGeneratedAndPassedThrough()
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