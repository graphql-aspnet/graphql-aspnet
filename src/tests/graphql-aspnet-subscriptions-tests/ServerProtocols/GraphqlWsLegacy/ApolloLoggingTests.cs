// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.GraphqlWsLegacy
{
    using GraphQL.AspNet.GraphqlWsLegacy.Logging.GraphqlWsLegacyEvents;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.ClientMessages;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.ServerMessages;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphqlWsLegacyLoggingTests
    {
        [Test]
        public void ClientMessageReceived_PropertyCheck()
        {
            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns("client1");

            var message = new GraphqlWsLegacyClientConnectionInitMessage();

            var entry = new GraphqlWsLegacyClientMessageReceivedLogEntry(client.Object, message);

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

            var message = new GraphqlWsLegacyServerDataMessage("123", result.Object);

            var entry = new GraphqlWsLegacyClientMessageSentLogEntry(client.Object, message);

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

            var entry = new GraphqlWsLegacyClientSubscriptionCreatedLogEntry(client.Object, sub.Object);

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

            var entry = new GraphqlWsLegacyClientSubscriptionStoppedLogEntry(client.Object, sub.Object);

            Assert.AreEqual("client1", entry.ClientId);
            Assert.AreEqual("sub1", entry.SubscriptionId);
            Assert.AreEqual("[subscription]/bobSub1", entry.Route);
            Assert.AreNotEqual(entry.ToString(), entry.GetType().Name);
        }
    }
}