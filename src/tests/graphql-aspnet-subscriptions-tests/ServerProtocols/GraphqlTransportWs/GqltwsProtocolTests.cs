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
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
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
                    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
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
        public async Task ReceiveEvent_WithNoClients_YieldsNothing()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            var evt = new SubscriptionEvent()
            {
                Data = null,
                DataTypeName = typeof(TwoPropertyObject).FullName,
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "fakeEvent",
            };

            var count = await server.ReceiveEvent(evt);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ReceiveEvent_WithNoData_YieldsNothing()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();
            var count = await server.ReceiveEvent(null);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task WhenTheServerReceivesAnEvent_WithARegisteredClient_ClientRecievesEvent()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            // start the sub on the client
            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            };

            // register a subscription
            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessMessage(startMessage);

            var evt = new SubscriptionEvent()
            {
                Data = new TwoPropertyObject(),
                DataTypeName = SchemaExtensions.RetrieveFullyQualifiedTypeName(typeof(TwoPropertyObject)),
                SchemaTypeName = SchemaExtensions.RetrieveFullyQualifiedTypeName(typeof(GraphSchema)),
                EventName = "[subscription]/GqltwsSubscription/WatchForPropObject",
            };

            // mimic new data available for that subscription
            var count = await server.ReceiveEvent(evt);

            Assert.AreEqual(1, count);
            connection.AssertGqltwsResponse(GqltwsMessageType.NEXT, "abc");
        }
    }
}