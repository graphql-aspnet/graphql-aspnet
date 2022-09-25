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
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS.GraphQLWSData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GQLWSProtocolTests
    {
        private async Task<(ISubscriptionServer<GraphSchema>, MockClientConnection, GQLWSClientProxy<GraphSchema>)> CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GQLWSSubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
                })
                .Build();

            var connection = server.CreateClientConnection();
            var serverOptions = server.ServiceProvider.GetRequiredService<SubscriptionServerOptions<GraphSchema>>();
            var subServer = server.ServiceProvider.GetRequiredService<ISubscriptionServer<GraphSchema>>();

            var subClient = new GQLWSClientProxy<GraphSchema>(
                connection,
                serverOptions,
                new GQLWSMessageConverterFactory());

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
        public async Task ReceiveEvent_WithARegisteredClient_ClientRecievesEvent()
        {
            (var server, var connection, var graphqlWsClient) = await this.CreateConnection();

            // start the sub on the client
            var startMessage = new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            };

            await connection.OpenAsync(SubscriptionConstants.WebSockets.GRAPHQL_WS_PROTOCOL);
            await graphqlWsClient.ProcessReceivedMessage(startMessage);

            var evt = new SubscriptionEvent()
            {
                Data = new TwoPropertyObject(),
                DataTypeName = SchemaExtensions.RetrieveFullyQualifiedDataObjectTypeName(typeof(TwoPropertyObject)),
                SchemaTypeName = SchemaExtensions.RetrieveFullyQualifiedSchemaTypeName(typeof(GraphSchema)),
                EventName = "[subscription]/GQLWSSubscription/WatchForPropObject",
            };

            var count = await server.ReceiveEvent(evt);
            Assert.AreEqual(1, count);

            connection.AssertGQLWSResponse(GQLWSMessageType.NEXT, "abc");
        }
    }
}