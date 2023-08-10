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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.SubscriptionServer;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ClientProxyLoggingTests
    {
        [Test]
        public void ClientMessageReceived_PropertyCheck()
        {
            var client = Substitute.For<ISubscriptionClientProxy>();

            var id = SubscriptionClientId.NewClientId();
            client.Id.Returns(id);

            var message = Substitute.For<ILoggableClientProxyMessage>();
            message.Type.Returns("typeABC");
            message.Id.Returns("idABC");

            var entry = new ClientProxyMessageReceivedLogEntry(client, message);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("typeABC", entry.MessageType);
            Assert.AreEqual("idABC", entry.MessageId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientMessageSent_PropertyCheck()
        {
            var client = Substitute.For<ISubscriptionClientProxy>();
            var result = Substitute.For<IQueryExecutionResult>();

            var id = SubscriptionClientId.NewClientId();
            client.Id.Returns(id);

            var message = Substitute.For<ILoggableClientProxyMessage>();
            message.Type.Returns("typeABC");
            message.Id.Returns("idABC");

            var entry = new ClientProxyMessageSentLogEntry(client, message);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("typeABC", entry.MessageType);
            Assert.AreEqual("idABC", entry.MessageId);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientSubscriptionCreated_PropertyCheck()
        {
            var client = Substitute.For<ISubscriptionClientProxy>();

            var id = SubscriptionClientId.NewClientId();
            client.Id.Returns(id);

            var sub = Substitute.For<ISubscription>();
            sub.Id.Returns("sub1");
            sub.Route.Returns(new SchemaItemPath("[subscription]/bobSub1"));

            var entry = new ClientProxySubscriptionCreatedLogEntry(client, sub);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.SubscriptionPath);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void ClientSubscriptionStopped_PropertyCheck()
        {
            var client = Substitute.For<ISubscriptionClientProxy>();

            var id = SubscriptionClientId.NewClientId();
            client.Id.Returns(id);

            var sub = Substitute.For<ISubscription>();
            sub.Id.Returns("sub1");
            sub.Route.Returns(new SchemaItemPath("[subscription]/bobSub1"));

            var entry = new ClientProxySubscriptionStoppedLogEntry(client, sub);

            Assert.AreEqual(id.ToString(), entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.SubscriptionPath);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }

        [Test]
        public void GraphQLWSClientSubscriptionEventReceived_PropertyCheck()
        {
            var connection = Substitute.For<IClientConnection>();
            var proxy = Substitute.For<ISubscriptionClientProxy<GraphSchema>>();

            var id = SubscriptionClientId.NewClientId();
            proxy.Id.Returns(id);

            var sub = Substitute.For<ISubscription>();
            sub.Id.Returns("sub1");

            var subs = new List<ISubscription>();
            subs.Add(sub);

            var fieldPath = new SchemaItemPath("[subscription]/bob1");

            var entry = new ClientProxySubscriptionEventReceived<GraphSchema>(
                proxy,
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