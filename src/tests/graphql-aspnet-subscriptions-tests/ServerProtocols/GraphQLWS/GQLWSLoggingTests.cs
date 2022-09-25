// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging.Events;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GQLWSLoggingTests
    {
        [Test]
        public void ClientMessageReceived_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns("client1");

            var message = new GQLWSClientConnectionInitMessage();

            var entry = new GQLWSClientMessageReceivedLogEntry(client.Object, message);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual(message.Type.ToString(), entry.MessageType);
            Assert.AreEqual(message.Id, entry.MessageId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientMessageSent_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            var result = new Mock<IGraphOperationResult>();
            client.Setup(x => x.Id).Returns("client1");

            var message = new GQLWSServerDataMessage("123", result.Object);

            var entry = new GQLWSClientMessageSentLogEntry(client.Object, message);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual(message.Type.ToString(), entry.MessageType);
            Assert.AreEqual(message.Id, entry.MessageId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientSubscriptionCreated_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns("client1");

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");
            sub.Setup(x => x.Route).Returns(new SchemaItemPath("[subscription]/bobSub1"));

            var entry = new GQLWSClientSubscriptionCreatedLogEntry(client.Object, sub.Object);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.Route);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientSubscriptionStopped_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns("client1");

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");
            sub.Setup(x => x.Route).Returns(new SchemaItemPath("[subscription]/bobSub1"));

            var entry = new GQLWSClientSubscriptionStoppedLogEntry(client.Object, sub.Object);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.Route);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void GraphQLWSEventMonitorEnded_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new GQLWSSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var eventName = new SubscriptionEventName("schema", "event");

            var entry = new GQLWSServerEventMonitorEndedLogEntry<GraphSchema>(server, eventName);

            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(eventName.ToString(), entry.SubscriptionEventName);
            Assert.AreEqual(server.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void GraphQLWSEventMonitorStarted_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new GQLWSSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var eventName = new SubscriptionEventName("schema", "event");

            var entry = new GQLWSServerEventMonitorStartedLogEntry<GraphSchema>(server, eventName);

            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(eventName.ToString(), entry.SubscriptionEventName);
            Assert.AreEqual(server.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void GraphQLWSServerSubscriptionEventReceived_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new GQLWSSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var eventData = new SubscriptionEvent()
            {
                Id = "id1",
                SchemaTypeName = "schema1",
                EventName = "event1",
                DataTypeName = "data1",
                Data = new object(),
            };

            var connection = new Mock<IClientConnection>();
            var proxy = new GQLWSClientProxy<GraphSchema>(
                connection.Object,
                new SubscriptionServerOptions<GraphSchema>(),
                new GQLWSMessageConverterFactory());

            var clients = new List<GQLWSClientProxy<GraphSchema>>();
            clients.Add(proxy);

            var entry = new GQLWSServerSubscriptionEventReceived<GraphSchema>(
                server,
                eventData,
                clients);

            Assert.AreEqual(eventData.SchemaTypeName, entry.SchemaTypeName);
            Assert.AreEqual(eventData.EventName, entry.SubscriptionEventName);
            Assert.AreEqual(1, entry.ClientCount);
            Assert.AreEqual("id1", entry.SubscriptionEventId);
            CollectionAssert.AreEquivalent(clients.Select(x => x.Id).ToList<string>(), entry.ClientIds);
            Assert.AreEqual(server.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void GraphQLWSClientSubscriptionEventReceived_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new GQLWSSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var connection = new Mock<IClientConnection>();
            var proxy = new GQLWSClientProxy<GraphSchema>(
                connection.Object,
                new SubscriptionServerOptions<GraphSchema>(),
                new GQLWSMessageConverterFactory());

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");

            var subs = new List<ISubscription>();
            subs.Add(sub.Object);

            var fieldPath = new SchemaItemPath("[subscription]/bob1");

            var entry = new GQLWSClientSubscriptionEventReceived<GraphSchema>(
                proxy,
                fieldPath,
                subs);

            Assert.AreEqual(proxy.Id, entry.ClientId);
            Assert.AreEqual(fieldPath.ToString(), entry.SubscriptionRoute);
            Assert.AreEqual(1, entry.SubscriptionCount);
            CollectionAssert.AreEquivalent(subs.Select(x => x.Id).ToList(), entry.SubscriptionIds);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }
    }
}