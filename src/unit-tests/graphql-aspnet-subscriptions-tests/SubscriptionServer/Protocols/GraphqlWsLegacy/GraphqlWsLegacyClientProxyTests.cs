// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Messages;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Mocks;
    using GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy.GraphqlWsLegacyData;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public partial class GraphqlWsLegacyClientProxyTests
    {
        private (MockClientConnection ClientConnection, GraphqlWsLegacyClientProxy<GraphSchema> ClientProxy, ISubscriptionEventRouter Router) CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddController<GraphqlWsLegacySubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromMinutes(15);
                    options.AuthenticatedRequestsOnly = false;
                })
                .Build();

            var router = Substitute.For<ISubscriptionEventRouter>();

            var connection = server.CreateClientConnection(GraphqlWsLegacyConstants.PROTOCOL_NAME);

            var subClient = new GraphqlWsLegacyClientProxy<GraphSchema>(
                server.Schema,
                connection,
                router,
                GraphqlWsLegacyConstants.PROTOCOL_NAME,
                server.ServiceProvider.GetService<IQueryResponseWriter<GraphSchema>>());

            return (connection, subClient, router);
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();

            Assert.AreNotEqual(Guid.Empty, client.Id);
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();

            // execute the connection sequence
            socketClient.QueueConnectionClosedByClient();

            await client.StartConnectionAsync();

            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueConnectionClosedByClient();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await client.StartConnectionAsync();

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertClientClosedConnection();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
           {
               await client.StartConnectionAsync();
           });
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.StartConnectionAsync();
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertClientClosedConnection();

            router.Received(1).AddClient(client, Arg.Any<SubscriptionEventName>());
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_YieldsDataMessageAndComplete()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.ProcessMessageAsync(startMessage);
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
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.ProcessMessageAsync(startMessage);

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

            await client.ReceiveEventAsync(evt);

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
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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

            await client.ReceiveEventAsync(evt);
            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventName_YieldsNothing()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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

            await client.ProcessMessageAsync(startMessage);

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
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.StartConnectionAsync();

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.COMPLETE, "abc");
            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task StopSubscription_AgainstNonExistantId_YieldsError()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStopMessage("abc123"));
            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnectionAsync(TimeSpan.FromMilliseconds(30));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.StartConnectionAsync(TimeSpan.FromSeconds(2));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertClientClosedConnection();

            router.Received(1).AddClient(client, Arg.Any<SubscriptionEventName>());
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.StartConnectionAsync(TimeSpan.FromSeconds(2));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task SendConnectionTerminate_ClosesConnectionFromServer()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionTerminateMessage());

            await client.StartConnectionAsync();

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertServerClosedConnection();
        }

        [Test]
        public async Task InvalidMessageType_ResultsInError()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new FakeGraphqlWsLegacyMessage()
            {
                Type = "invalid_type",
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await client.StartConnectionAsync(TimeSpan.FromSeconds(2));
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
            socketClient.AssertClientClosedConnection();
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
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
            await client.StartConnectionAsync(TimeSpan.FromSeconds(2));

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

        [Test]
        public async Task Deserialize_InvalidMessage_ProcessesUnknownMessage()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var socketClient, var client, var router) = this.CreateConnection();
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new MockSocketMessage("{ Non-deserialziable Json"));

            socketClient.QueueConnectionClosedByClient();
            await client.StartConnectionAsync(TimeSpan.FromSeconds(2));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);

            // server responds with unknown message
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
            socketClient.AssertClientClosedConnection();
        }
    }
}