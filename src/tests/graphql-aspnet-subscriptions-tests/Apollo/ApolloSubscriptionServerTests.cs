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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Apollo.ApolloTestData;
    using GraphQL.Subscriptions.Tests.CommonHelpers;
    using GraphQL.Subscriptions.Tests.TestServerHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloSubscriptionServerTests
    {
        [Test]
        public async Task WhenConnectionEstablished_RequiredMessagesReturned()
        {
            var socketClient = new MockClientConnection();
            var options = new SchemaSubscriptionOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                new Mock<ISubscriptionEventListener<GraphSchema>>().Object);

            subscriptionServer.RegisterNewClient(apolloClient);

            var message = new ApolloConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new MockClientMessage(new ApolloConnectionInitMessage()));
            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await apolloClient.StartConnection();

            // the server should have sent back two messages to the client (ack and keep alive) per the protocol
            Assert.AreEqual(2, socketClient.ResponseMessageCount);

            // ensure the two response messages are of the appropriate type
            socketClient.AssertServerSentMessageType(ApolloMessageType.CONNECTION_ACK, true);
            socketClient.AssertServerSentMessageType(ApolloMessageType.CONNECTION_KEEP_ALIVE, true);
        }

        [Test]
        public async Task WhenSubscriptionStarted_GeneratesNewSubscriptionRegistration()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ApolloSubscriptionController>()
                .AddSubscriptions()
                .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var message = new ApolloConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new ApolloConnectionInitMessage());
            socketClient.QueueNewSubscription(
                "abc123",
                "subscription { apolloSubscription { watchForPropObject { property1 property2} } }");

            socketClient.QueueConnectionCloseMessage();

            bool eventTriggered = false;

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                new Mock<ISubscriptionEventListener<GraphSchema>>().Object);

            subscriptionServer.SubscriptionRegistered += (sender, args) =>
            {
                eventTriggered = true;
                Assert.AreEqual(1, subscriptionServer.Subscriptions.RetrieveSubscriptions(apolloClient).Count());
                Assert.AreEqual(1, subscriptionServer.Subscriptions.RetrieveSubscriptions("[subscription]/ApolloSubscription/WatchForPropObject").Count());
            };

            subscriptionServer.RegisterNewClient(apolloClient);

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.IsTrue(eventTriggered, "NewSub event never fired");
        }

        [Test]
        public async Task WhenSubscriptionStoped_SubscriptionRegistrationIsDropped()
        {
            var testServer = new TestServerBuilder()
              .AddGraphController<ApolloSubscriptionController>()
              .AddSubscriptions()
              .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var message = new ApolloConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new ApolloConnectionInitMessage());
            socketClient.QueueNewSubscription(
                "abc123",
                "subscription { apolloSubscription { watchForPropObject { property1 property2} } }");

            socketClient.QueueClientMessage(new ApolloSubscriptionStopMessage("abc123"));
            socketClient.QueueConnectionCloseMessage();

            bool subscribeEventFired = false;
            bool unsubscribeEventFired = false;

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                new Mock<ISubscriptionEventListener<GraphSchema>>().Object);

            subscriptionServer.SubscriptionRegistered += (sender, args) =>
            {
                subscribeEventFired = true;
            };

            subscriptionServer.SubscriptionRemoved += (sender, args) =>
            {
                unsubscribeEventFired = true;
                Assert.AreEqual(0, subscriptionServer.Subscriptions.RetrieveSubscriptions(apolloClient).Count());
                Assert.AreEqual(0, subscriptionServer.Subscriptions.RetrieveSubscriptions("[subscription]/ApolloSubscription/WatchForPropObject").Count());
            };

            subscriptionServer.RegisterNewClient(apolloClient);

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.IsTrue(subscribeEventFired, "Subscribe event never fired");
            Assert.IsTrue(unsubscribeEventFired, "Unsubscribe never fired");
        }
    }
}