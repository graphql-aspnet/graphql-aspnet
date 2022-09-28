// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GqltwsProtocolTests
    {
        private async Task<(ISubscriptionServer<GraphSchema>, MockClientConnection, GqltwsClientProxy<GraphSchema>)> CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GqltwsSubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromMinutes(15);
                })
                .Build();

            var connection = server.CreateClientConnection();
            var serverOptions = server.ServiceProvider.GetRequiredService<SubscriptionServerOptions<GraphSchema>>();
            var subServer = server.ServiceProvider.GetRequiredService<ISubscriptionServer<GraphSchema>>();

            var subClient = new GqltwsClientProxy<GraphSchema>(connection);

            var graphqlWsClient = await subServer.RegisterNewClient(subClient);
            return (subServer,
                connection,
                subClient);
        }

        [Test]
        public async Task SendingTooManyInitRequests_ClosesTheConnection()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());

            await graphqlWsClient.StartConnection();

            // a response to the first message should have been transmitted
            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, GqltwsConstants.CustomCloseEventIds.TooManyInitializationRequests);
        }

        [Test]
        public async Task NotSendingTheInitMessageWithinTimeoutPeriod_ClosesTheConnection()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            await graphqlWsClient.StartConnection(initializationTimeout: TimeSpan.FromMilliseconds(5));

            // no response should ever have been given
            // and the connection should have been closed from the server
            Assert.AreEqual(1, connection.ResponseMessageCount);
            connection.AssertServerClosedConnection();

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, GqltwsConstants.CustomCloseEventIds.ConnectionInitializationTimeout);
        }

        [Test]
        public async Task WhenPingReceived_PongIsSent()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsPingMessage());
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnection();

            // no response should ever have been given
            // and the connection should have been closed from the server
            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.PONG);

            // no other messages
            Assert.AreEqual(0, connection.ResponseMessageCount);

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, (int)ConnectionCloseStatus.NormalClosure);
        }

        [Test]
        public async Task WhenClientSendsSubscriptionComplete_ServerDropsTheSubscription()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            // queue the subscription
            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            // ensure it was set up
            connection.QueueAction(() =>
            {
                Assert.AreEqual(1, graphqlWsClient.Subscriptions.Count());
            });

            // unsbuscribe the subscription
            connection.QueueClientMessage((object)new GqltwsSubscriptionCompleteMessage("abc"));

            // ensure it was removed
            connection.QueueAction(() =>
            {
                Assert.AreEqual(0, graphqlWsClient.Subscriptions.Count());
            });

            // close out
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnection();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);

            Assert.AreEqual(0, graphqlWsClient.Subscriptions.Count());
            Assert.AreEqual(0, connection.ResponseMessageCount);

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, (int)ConnectionCloseStatus.NormalClosure);
        }

        [Test]
        public void RecievingAnInvaidMessageType_ClosesSocketImmediately()
        {
            Assert.Fail("Write this test");
        }
    }
}