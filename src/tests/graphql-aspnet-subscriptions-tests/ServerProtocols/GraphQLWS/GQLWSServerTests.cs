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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ClientMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS.GraphQLWSData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GQLWSServerTests
    {
        private async Task<(GQLWSSubscriptionServer<GraphSchema>, MockClientConnection, GQLWSClientProxy<GraphSchema>)>
            CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GQLWSSubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
                })
                .Build();

            var socketClient = server.CreateClient();
            var subServer = server.ServiceProvider.GetRequiredService<ISubscriptionServer<GraphSchema>>();

            var graphqlWsClient = await subServer.RegisterNewClient(socketClient);
            return (subServer as GQLWSSubscriptionServer<GraphSchema>,
                socketClient,
                graphqlWsClient as GQLWSClientProxy<GraphSchema>);
        }

        [Test]
        public async Task ReceiveEvent_WithNoClients_YieldsNothing()
        {
            (var server, var socketClient, var graphqlWsClient) = await this.CreateConnection();

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
            (var server, var socketClient, var graphqlWsClient) = await this.CreateConnection();
            var count = await server.ReceiveEvent(null);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ReceiveEvent_WithARegisteredClient_ClientRecievesEvent()
        {
            (var server, var socketClient, var graphqlWsClient) = await this.CreateConnection();

            // start the sub on the client
            var startMessage = new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            };

            await graphqlWsClient.DispatchMessage(startMessage);

            var evt = new SubscriptionEvent()
            {
                Data = new TwoPropertyObject(),
                DataTypeName = SchemaExtensions.RetrieveFullyQualifiedDataObjectTypeName(typeof(TwoPropertyObject)),
                SchemaTypeName = SchemaExtensions.RetrieveFullyQualifiedSchemaTypeName(typeof(GraphSchema)),
                EventName = "[subscription]/GQLWSSubscription/WatchForPropObject",
            };

            var count = await server.ReceiveEvent(evt);
            Assert.AreEqual(1, count);

            socketClient.AssertGQLWSResponse(GQLWSMessageType.DATA, "abc");
        }
    }
}