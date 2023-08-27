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
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
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
            var mock = Substitute.For<IGraphEventLogger>();
            mock.When(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<Func<IGraphLogEntry>>()))
                .Do(x =>
                {
                    recordedLogLevel = (LogLevel)x[0];
                    recordedlogEntry = ((Func<IGraphLogEntry>)x[1])();
                });

            mock.SchemaSubscriptionRouteRegistered<GraphSchema>("testPath");

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
            var mock = Substitute.For<IGraphEventLogger>();
            mock.When(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<Func<IGraphLogEntry>>()))
               .Do(x =>
               {
                   recordedLogLevel = (LogLevel)x[0];
                   recordedlogEntry = ((Func<IGraphLogEntry>)x[1])();
               });

            var client = Substitute.For<ISubscriptionClientProxy<GraphSchema>>();

            var id = SubscriptionClientId.NewClientId();
            client.Id.Returns(id);

            mock.SubscriptionClientRegistered<GraphSchema>(client);

            var entry = recordedlogEntry as SubscriptionClientRegisteredLogEntry<GraphSchema>;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(client.GetType().FriendlyName(true), entry.ClientTypeName);
            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionClientDropped_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = Substitute.For<IGraphEventLogger>();
            mock.When(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<Func<IGraphLogEntry>>()))
               .Do(x =>
               {
                   recordedLogLevel = (LogLevel)x[0];
                   recordedlogEntry = ((Func<IGraphLogEntry>)x[1])();
               });

            var client = Substitute.For<ISubscriptionClientProxy<GraphSchema>>();

            var id = SubscriptionClientId.NewClientId();
            client.Id.Returns(id);

            mock.SubscriptionClientDropped(client);

            var entry = recordedlogEntry as SubscriptionClientDroppedLogEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(LogLevel.Debug, recordedLogLevel);
            Assert.AreEqual(client.GetType().FriendlyName(true), entry.ClientTypeName);
            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void SubscriptionEventReceived_EnsureEventEntryIsGeneratedAndPassedThrough()
        {
            LogLevel recordedLogLevel = LogLevel.None;
            IGraphLogEntry recordedlogEntry = null;

            // fake a log to recieve the event generated on the extension method
            var mock = Substitute.For<ILogger>();
            mock.When(x => x.Log(
                Arg.Any<LogLevel>(),
                Arg.Any<EventId>(),
                Arg.Any<SubscriptionEventReceivedLogEntry>(),
                null,
                Arg.Any<Func<SubscriptionEventReceivedLogEntry, Exception, string>>()))
                .Do(x =>
                {
                    recordedLogLevel = (LogLevel)x[0];
                    recordedlogEntry = (SubscriptionEventReceivedLogEntry)x[2];
                });

            mock.IsEnabled(LogLevel.Debug).Returns(true);

            var eventData = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                SchemaTypeName = "someSchemaName",
                Data = new object(),
                DataTypeName = "someTypeName",
                EventName = "testEvent",
            };

            mock.SubscriptionEventReceived(eventData);

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
            var mock = Substitute.For<IGraphEventLogger>();
            mock.When(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<Func<IGraphLogEntry>>()))
                  .Do(x =>
                  {
                      recordedLogLevel = (LogLevel)x[0];
                      recordedlogEntry = ((Func<IGraphLogEntry>)x[1])();
                  });

            var eventData = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                SchemaTypeName = "someSchemaName",
                Data = new object(),
                DataTypeName = "someTypeName",
                EventName = "testEvent",
            };

            mock.SubscriptionEventPublished(eventData);

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