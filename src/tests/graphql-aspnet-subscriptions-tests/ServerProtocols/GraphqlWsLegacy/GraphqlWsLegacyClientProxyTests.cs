// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlWsLegacy
{
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ServerMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlWsLegacy.GraphqlWsLegacyData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public partial class GraphqlWsLegacyClientProxyTests
    {
        private (MockClientConnection, GraphqlWsLegacyClientProxy<GraphSchema>, Mock<ISubscriptionEventRouter>) CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GraphqlWsLegacySubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromMinutes(15);
                    options.AuthenticatedRequestsOnly = false;
                })
                .Build();

            var router = new Mock<ISubscriptionEventRouter>();

            var connection = server.CreateClientConnection(GraphqlWsLegacyConstants.PROTOCOL_NAME);

            var subClient = new GraphqlWsLegacyClientProxy<GraphSchema>(
                server.Schema,
                connection,
                router.Object,
                GraphqlWsLegacyConstants.PROTOCOL_NAME);
            return (connection, subClient, router);
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            Assert.IsNotNull(string.IsNullOrWhiteSpace(client.Id));
            Assert.AreNotEqual(Guid.Empty.ToString(), client.Id);
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            // execute the connection sequence
            socketClient.QueueConnectionClosedByClient();

            await client.StartConnection();

            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueConnectionClosedByClient();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await client.StartConnection();

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertClientClosedConnection();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
           {
               await client.StartConnection();
           });
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnection();
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertClientClosedConnection();

            router.Verify(x => x.AddReceiver(client, It.IsAny<SubscriptionEventName>()), Times.Once());
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_YieldsDataMessageAndComplete()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            // use start message to send a query, not a subscription request
            // client should respond with expected data
            // then a complete
            var startMessage = new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query {  fastQuery { property1 } }",
                },
            };

            await socketClient.OpenAsync(GraphqlWsLegacyConstants.PROTOCOL_NAME);
            await client.ProcessMessage(startMessage);
            socketClient.AssertGraphqlWsLegacyResponse(
                GraphqlWsLegacyMessageType.DATA,
                "abc",
                @"{
                    ""data"" : {
                        ""fastQuery"" : {
                            ""property1"" : ""bob""
                        }
                    }
                }");

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.COMPLETE);
        }

        [Test]
        public async Task ReceiveEvent_OnStartedSubscription_YieldsDataMessage()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            var startMessage = new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            };

            await socketClient.OpenAsync(GraphqlWsLegacyConstants.PROTOCOL_NAME);
            await client.ProcessMessage(startMessage);

            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GraphqlWsLegacySubscriptionController.WatchForPropObject),
                SchemaTypeName = new GraphSchema().FullyQualifiedSchemaTypeName(),
            };

            await client.ReceiveEvent(evt);

            socketClient.AssertGraphqlWsLegacyResponse(
                GraphqlWsLegacyMessageType.DATA,
                "abc",
                @"{
                    ""data"" : {
                        ""graphqlWsLegacySubscription"" : {
                            ""watchForPropObject"" : {
                                ""property1"" : ""value1"",
                            }
                        }
                    }
                }");
        }

        [Test]
        public async Task ReceiveEvent_WhenNoSubscriptions_YieldsNothing()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GraphqlWsLegacySubscriptionController.WatchForPropObject),
                SchemaTypeName = nameof(GraphSchema),
            };

            await client.ReceiveEvent(evt);

            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventNAme_YieldsNothing()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            // start a real subscription so the client is tracking one
            var startMessage = new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            };

            await client.ProcessMessage(startMessage);

            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GraphqlWsLegacySubscriptionController.WatchForPropObject2),
                SchemaTypeName = nameof(GraphSchema),
            };

            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task StopSubscription_AgainstExistantId_RemovesSubscriptionCorrectly()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueAction(() =>
            {
                Assert.AreEqual(1, client.Subscriptions.Count());
            });

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStopMessage("abc"));

            socketClient.QueueAction(() =>
            {
                Assert.AreEqual(0, client.Subscriptions.Count());
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnection();

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.COMPLETE, "abc");
            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task StopSubscription_AgainstNonExistantId_YieldsError()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStopMessage("abc123"));
            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnection(TimeSpan.FromMilliseconds(30));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc1",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueAction(() =>
            {
                Assert.AreEqual(2, client.Subscriptions.Count());
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnection(TimeSpan.FromSeconds(2));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertClientClosedConnection();

            router.Verify(x => x.AddReceiver(client, It.IsAny<SubscriptionEventName>()), Times.Once());
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueAction(() =>
            {
                Assert.AreEqual(1, client.Subscriptions.Count());
            });

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueAction(() =>
            {
                // first sbuscription should still be open
                Assert.AreEqual(1, client.Subscriptions.Count());
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnection(TimeSpan.FromSeconds(2));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task SendConnectionTerminate_ClosesConnectionFromServer()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionTerminateMessage());

            await client.StartConnection();

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertServerClosedConnection();
        }

        [Test]
        public async Task InvalidMessageType_ResultsInError()
        {
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new FakeGraphqlWsLegacyMessage()
            {
                Type = "invalid_type",
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnection(TimeSpan.FromSeconds(2));
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            (var socketClient, var client, var router) = this.CreateConnection();
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query { fastQuery { property1 } }",
                },
            });

            socketClient.QueueConnectionClosedByClient();
            await client.StartConnection(TimeSpan.FromSeconds(2));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(
                GraphqlWsLegacyMessageType.DATA,
                "abc",
                @"{
                    ""data"" : {
                        ""fastQuery"" : {
                            ""property1"" : ""bob""
                        }
                    }
                }");

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.COMPLETE);

            socketClient.AssertClientClosedConnection();
        }
    }
}