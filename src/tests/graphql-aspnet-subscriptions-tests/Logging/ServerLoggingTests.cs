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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.SubscriptionEventLogEntries;
    using GraphQL.AspNet.Schemas;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ServerLoggingTests
    {
        [Test]
        public void GraphQLWSEventMonitorEnded_PropertyCheck()
        {
            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

            var eventName = new SubscriptionEventName("schema", "event");

            var entry = new SubscriptionServerEventMonitorEndedLogEntry<GraphSchema>(server.Object, eventName);

            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(eventName.ToString(), entry.SubscriptionEventName);
            Assert.AreEqual(server.Object.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void GraphQLWSEventMonitorStarted_PropertyCheck()
        {
            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

            var eventName = new SubscriptionEventName("schema", "event");

            var entry = new SubscriptionServerEventMonitorStartedLogEntry<GraphSchema>(server.Object, eventName);

            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(eventName.ToString(), entry.SubscriptionEventName);
            Assert.AreEqual(server.Object.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void GraphQLWSServerSubscriptionEventReceived_PropertyCheck()
        {
            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

            var eventData = new SubscriptionEvent()
            {
                Id = "id1",
                SchemaTypeName = "schema1",
                EventName = "event1",
                DataTypeName = "data1",
                Data = new object(),
            };

            var connection = new Mock<IClientConnection>();
            var proxy = new Mock<ISubscriptionClientProxy<GraphSchema>>();
            proxy.Setup(x => x.Id).Returns("client1");

            var clients = new List<ISubscriptionClientProxy<GraphSchema>>();
            clients.Add(proxy.Object);

            var entry = new SubscriptionServerSubscriptionEventReceived<GraphSchema>(
                server.Object,
                eventData,
                clients);

            Assert.AreEqual(eventData.SchemaTypeName, entry.SchemaTypeName);
            Assert.AreEqual(eventData.EventName, entry.SubscriptionEventName);
            Assert.AreEqual(1, entry.ClientCount);
            Assert.AreEqual("id1", entry.SubscriptionEventId);
            CollectionAssert.AreEquivalent(clients.Select(x => x.Id).ToList<string>(), entry.ClientIds);
            Assert.AreEqual(server.Object.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }
    }
}