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
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.Subscriptions.Tests.Apollo.ApolloTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using NUnit.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages;
    using GraphQL.AspNet.Common.Extensions;

    [TestFixture]
    public class ApolloServerTests
    {
        private async Task<(ApolloSubscriptionServer<GraphSchema>, MockClientConnection, ApolloClientProxy<GraphSchema>)>
            CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<ApolloSubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
                })
                .Build();

            var socketClient = server.CreateClient();
            var subServer = server.ServiceProvider.GetRequiredService<ISubscriptionServer<GraphSchema>>();

            var apolloClient = await subServer.RegisterNewClient(socketClient);
            return (subServer as ApolloSubscriptionServer<GraphSchema>,
                socketClient,
                apolloClient as ApolloClientProxy<GraphSchema>);
        }

        [Test]
        public async Task ReceiveEvent_WithNoClients_YieldsNothing()
        {
            (var server, var socketClient, var apolloClient) = await this.CreateConnection();

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
            (var server, var socketClient, var apolloClient) = await this.CreateConnection();
            var count = await server.ReceiveEvent(null);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ReceiveEvent_WithARegisteredClient_ClientRecievesEvent()
        {
            (var server, var socketClient, var apolloClient) = await this.CreateConnection();

            // start the sub on the client
            var startMessage = new ApolloClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            };

            await apolloClient.DispatchMessage(startMessage);

            var evt = new SubscriptionEvent()
            {
                Data = new TwoPropertyObject(),
                DataTypeName = SchemaExtensions.RetrieveFullyQualifiedDataObjectTypeName(typeof(TwoPropertyObject)),
                SchemaTypeName = SchemaExtensions.RetrieveFullyQualifiedSchemaTypeName(typeof(GraphSchema)),
                EventName = "[subscription]/ApolloSubscription/WatchForPropObject",
            };

            var count = await server.ReceiveEvent(evt);
            Assert.AreEqual(1, count);

            socketClient.AssertApolloResponse(ApolloMessageType.DATA, "abc");
        }
    }
}