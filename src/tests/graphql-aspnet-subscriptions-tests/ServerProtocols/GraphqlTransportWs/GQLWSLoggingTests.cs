﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Logging.Events;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GqltwsLoggingTests
    {
        [Test]
        public void ClientMessageReceived_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns("client1");

            var message = new GqltwsClientConnectionInitMessage();

            var entry = new GqltwsClientMessageReceivedLogEntry(client.Object, message);

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

            var message = new GqltwsServerNextDataMessage("123", result.Object);

            var entry = new GqltwsClientMessageSentLogEntry(client.Object, message);

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

            var entry = new GqltwsClientSubscriptionCreatedLogEntry(client.Object, sub.Object);

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

            var entry = new GqltwsClientSubscriptionStoppedLogEntry(client.Object, sub.Object);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.Route);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void GraphQLWSClientSubscriptionEventReceived_PropertyCheck()
        {
            var connection = new Mock<IClientConnection>();
            var proxy = new GqltwsClientProxy<GraphSchema>(
                connection.Object,
                new SubscriptionServerOptions<GraphSchema>(),
                new GqltwsMessageConverterFactory());

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");

            var subs = new List<ISubscription>();
            subs.Add(sub.Object);

            var fieldPath = new SchemaItemPath("[subscription]/bob1");

            var entry = new GqltwsClientSubscriptionEventReceived<GraphSchema>(
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