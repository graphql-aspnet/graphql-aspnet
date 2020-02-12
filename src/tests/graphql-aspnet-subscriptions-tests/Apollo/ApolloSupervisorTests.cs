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
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Apollo.ApolloTestData;
    using GraphQL.Subscriptions.Tests.CommonHelpers;
    using GraphQL.Subscriptions.Tests.TestServerHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloSupervisorTests
    {
        [Test]
        public async Task Supervisor_WhenConnectionEstablished_RequiredMessagesReturned()
        {
            var socketClient = new MockClientConnection();
            var options = new SchemaSubscriptionOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

            var supervisor = new ApolloClientSupervisor<GraphSchema>();
            supervisor.RegisterNewClient(apolloClient);

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
        public async Task Supervisor_WhenSubscriptionStarted_GeneratesNewSubscriptionRegistration()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ApolloSubscriptionController>()
                .AddSubscriptions()
                .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var supervisor = new ApolloClientSupervisor<GraphSchema>();
            supervisor.RegisterNewClient(apolloClient);

            var message = new ApolloConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new ApolloConnectionInitMessage());
            socketClient.QueueNewSubscription(
                "abc123",
                "subscription { apolloSubscription { watchForPropObject { property1 property2} } }");

            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await apolloClient.StartConnection();

            var registeredSubs = supervisor.RetrieveSubscriptions(apolloClient);
            Assert.AreEqual(1, registeredSubs.Count());
        }

        [Test]
        public async Task Supervisor_WhenSubscriptionStoped_SubscriptionRegistrationIsDropped()
        {
            Assert.Inconclusive("Write this test");
        }
    }
}