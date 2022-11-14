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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.ClientProxyLogEntries;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ClientProxyLoggingTests
    {
        [Test]
        public void ClientMessageReceived_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();

            var id = Guid.NewGuid();
            client.Setup(x => x.Id).Returns(id);

            var message = new Mock<ILoggableClientProxyMessage>();
            message.Setup(x => x.Type).Returns("typeABC");
            message.Setup(x => x.Id).Returns("idABC");

            var entry = new ClientProxyMessageReceivedLogEntry(client.Object, message.Object);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("typeABC", entry.MessageType);
            Assert.AreEqual("idABC", entry.MessageId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientMessageSent_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            var result = new Mock<IGraphOperationResult>();

            var id = Guid.NewGuid();
            client.Setup(x => x.Id).Returns(id);

            var message = new Mock<ILoggableClientProxyMessage>();
            message.Setup(x => x.Type).Returns("typeABC");
            message.Setup(x => x.Id).Returns("idABC");

            var entry = new ClientProxyMessageSentLogEntry(client.Object, message.Object);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("typeABC", entry.MessageType);
            Assert.AreEqual("idABC", entry.MessageId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientSubscriptionCreated_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();

            var id = Guid.NewGuid();
            client.Setup(x => x.Id).Returns(id);

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");
            sub.Setup(x => x.Route).Returns(new SchemaItemPath("[subscription]/bobSub1"));

            var entry = new ClientProxySubscriptionCreatedLogEntry(client.Object, sub.Object);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.SubscriptionPath);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientSubscriptionStopped_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();

            var id = Guid.NewGuid();
            client.Setup(x => x.Id).Returns(id);

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");
            sub.Setup(x => x.Route).Returns(new SchemaItemPath("[subscription]/bobSub1"));

            var entry = new ClientProxySubscriptionStoppedLogEntry(client.Object, sub.Object);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.SubscriptionPath);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void GraphQLWSClientSubscriptionEventReceived_PropertyCheck()
        {
            var connection = new Mock<IClientConnection>();
            var proxy = new Mock<ISubscriptionClientProxy<GraphSchema>>();

            var id = Guid.NewGuid();
            proxy.Setup(x => x.Id).Returns(id);

            var sub = new Mock<ISubscription>();
            sub.Setup(x => x.Id).Returns("sub1");

            var subs = new List<ISubscription>();
            subs.Add(sub.Object);

            var fieldPath = new SchemaItemPath("[subscription]/bob1");

            var entry = new ClientProxySubscriptionEventReceived<GraphSchema>(
                proxy.Object,
                fieldPath,
                subs);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual(fieldPath.ToString(), entry.SubscriptionPath);
            Assert.AreEqual(1, entry.SubscriptionCount);
            CollectionAssert.AreEquivalent(subs.Select(x => x.Id).ToList(), entry.SubscriptionIds);
            Assert.AreNotEqual(entry.GetType().Name, entry.ToString());
        }
    }
}