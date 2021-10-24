// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Apollo
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Apollo.Logging.ApolloEvents;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloLoggingTests
    {
        [Test]
        public void ClientMessageReceived_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns("client1");

            var message = new ApolloClientConnectionInitMessage();

            var entry = new ApolloClientMessageReceivedLogEntry(client.Object, message);

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

            var message = new ApolloServerDataMessage("123", result.Object);

            var entry = new ApolloClientMessageSentLogEntry(client.Object, message);

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
            sub.Setup(x => x.Route).Returns(new GraphFieldPath("[subscription]/bobSub1"));

            var entry = new ApolloClientSubscriptionCreatedLogEntry(client.Object, sub.Object);

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
            sub.Setup(x => x.Route).Returns(new GraphFieldPath("[subscription]/bobSub1"));

            var entry = new ApolloClientSubscriptionStoppedLogEntry(client.Object, sub.Object);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.Route);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ApolloEventMonitorEnded_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new ApolloSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var eventName = new SubscriptionEventName("schema", "event");

            var entry = new ApolloServerEventMonitorEndedLogEntry<GraphSchema>(server, eventName);

            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(eventName.ToString(), entry.SubscriptionEventName);
            Assert.AreEqual(server.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void ApolloEventMonitorStarted_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new ApolloSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var eventName = new SubscriptionEventName("schema", "event");

            var entry = new ApolloServerEventMonitorStartedLogEntry<GraphSchema>(server, eventName);

            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(eventName.ToString(), entry.SubscriptionEventName);
            Assert.AreEqual(server.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void ApolloServerSubscriptionEventReceived_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new ApolloSubscriptionServer<GraphSchema>(
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
            var proxy = new ApolloClientProxy<GraphSchema>(
                connection.Object,
                new SubscriptionServerOptions<GraphSchema>(),
                new AspNet.Apollo.Messages.Converters.ApolloMessageConverterFactory());

            var clients = new List<ApolloClientProxy<GraphSchema>>();
            clients.Add(proxy);

            var entry = new ApolloServerSubscriptionEventReceived<GraphSchema>(
                server,
                eventData,
                clients);

            Assert.AreEqual(eventData.SchemaTypeName, entry.SchemaTypeName);
            Assert.AreEqual(eventData.EventName, entry.SubscriptionEventName);
            Assert.AreEqual(1, entry.ClientCount);
            Assert.AreEqual("id1", entry.SubscriptionEventId);
            CollectionAssert.AreEquivalent(clients.Select(x => x.Id).ToList(), entry.ClientIds);
            Assert.AreEqual(server.Id, entry.ServerId);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }

        [Test]
        public void ApolloClientSubscriptionEventReceived_PropertyCheck()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            var server = new ApolloSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                new SubscriptionServerOptions<GraphSchema>(),
                router.Object);

            var connection = new Mock<IClientConnection>();
            var proxy = new ApolloClientProxy<GraphSchema>(
                connection.Object,
                new SubscriptionServerOptions<GraphSchema>(),
                new AspNet.Apollo.Messages.Converters.ApolloMessageConverterFactory());

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");

            var subs = new List<ISubscription>();
            subs.Add(sub.Object);

            var fieldPath = new GraphFieldPath("[subscription]/bob1");

            var entry = new ApolloClientSubscriptionEventReceived<GraphSchema>(
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