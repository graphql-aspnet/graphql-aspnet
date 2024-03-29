﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlTransportWs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Tests.Mocks;
    using GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlTransportWs.GraphqlTransportWsData;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy;

    [TestFixture]
    public partial class GqltwsClientProxyTests
    {
        private (MockClientConnection ClientConnection, GqltwsClientProxy<GraphSchema> ClientProxy, ISubscriptionEventRouter Router) CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GqltwsSubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromMinutes(15);
                    options.AuthenticatedRequestsOnly = false;
                })
                .Build();

            var router = Substitute.For<ISubscriptionEventRouter>();

            var connection = server.CreateClientConnection(GqltwsConstants.PROTOCOL_NAME);
            var serverOptions = server.ServiceProvider.GetRequiredService<SubscriptionServerOptions<GraphSchema>>();

            var subClient = new GqltwsClientProxy<GraphSchema>(
                connection,
                server.Schema,
                router,
                server.ServiceProvider.GetService<IQueryResponseWriter<GraphSchema>>());
            return (connection, subClient, router);
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            Assert.AreNotEqual(Guid.Empty, graphqlWsClient.Id);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the graphql-ws client to do the close
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();

            // execute the connection sequence
            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnectionAsync();

            connection.AssertClientClosedConnection();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // init a connection then close the socket
            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueConnectionClosedByClient();

            Assert.AreEqual(2, connection.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();

            Assert.AreEqual(0, connection.QueuedMessageCount);

            // attempt to restart the closed connection
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await graphqlWsClient.StartConnectionAsync();
            });

            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartConnection_ContinuesToReadMessagesFromTheSocketConnect_UntilCloseMessage()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueConnectionClosedByClient(); // socket level close message

            Assert.AreEqual(2, connection.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();

            Assert.AreEqual(0, connection.QueuedMessageCount);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // startup the connection then register a subscription
            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();

            router.Received(1).AddClient(graphqlWsClient, Arg.Any<SubscriptionEventName>());
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_ImmediatelyYieldsNEXTMessage()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // Use subscribe message to send a query, not a subscription request.
            // Client should respond with expected NEXT with the results
            // then a complete message
            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query {  fastQuery { property1 } }",
                },
            };

            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessMessageAsync(startMessage);

            connection.AssertGqltwsResponse(
                  GqltwsMessageType.NEXT,
                  "abc",
                  @"{
                    ""data"" : {
                        ""fastQuery"" : {
                            ""property1"" : ""bob""
                        }
                    }
                }");

            connection.AssertGqltwsResponse(GqltwsMessageType.COMPLETE);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_WithSyntaxError_ImmediatelyYieldsError()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // Use subscribe message to send a query, not a subscription request.
            // Client should respond with expected NEXT with the results
            // then a complete message
            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query {  fastQuery { property1 }", // syntax error
                },
            };

            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessMessageAsync(startMessage);

            connection.AssertGqltwsResponse(GqltwsMessageType.ERROR, "abc");
            connection.AssertConnectionIsOpen();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ReceiveEvent_OnStartedSubscription_YieldsNEXTMessage()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            };

            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessMessageAsync(startMessage);

            // mimic new data for the registered subscription being processed by some
            // other mutation
            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GqltwsSubscriptionController.WatchForPropObject),
                SchemaTypeName = new GraphSchema().FullyQualifiedSchemaTypeName(),
            };

            await graphqlWsClient.ReceiveEventAsync(evt);

            // the connection should receive a data package
            connection.AssertGqltwsResponse(
                GqltwsMessageType.NEXT,
                "abc",
                @"{
                    ""data"" : {
                        ""gqltwsSubscription"" : {
                            ""watchForPropObject"" : {
                                ""property1"" : ""value1"",
                            }
                        }
                    }
                }");
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ReceiveEvent_WhenNoSubscriptions_YieldsNothing()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // no active subscriptions for the client, but a server event is received
            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GqltwsSubscriptionController.WatchForPropObject),
                SchemaTypeName = nameof(GraphSchema),
            };

            await graphqlWsClient.ReceiveEventAsync(evt);

            // nothing should have been sent to the connection
            Assert.AreEqual(0, connection.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventNAme_YieldsNothing()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // start a real subscription so the client is tracking one
            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            };

            await graphqlWsClient.ProcessMessageAsync(startMessage);

            // fire an event against a route not tracked, ensure the client skips it.
            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GqltwsSubscriptionController.WatchForPropObject2),
                SchemaTypeName = nameof(GraphSchema),
            };

            Assert.AreEqual(0, connection.ResponseMessageCount);
        }

        [Test]
        public async Task StopSubscription_RemovesSubscriptionCorrectly()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueClientMessage(new GqltwsSubscriptionCompleteMessage()
            {
                Id = "abc",
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc1",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.ERROR);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();
            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query { fastQuery { property1 } }",
                },
            });

            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(
                GqltwsMessageType.NEXT,
                "abc",
                @"{
                    ""data"" : {
                        ""fastQuery"" : {
                            ""property1"" : ""bob""
                        }
                    }
                }");
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task SendingTooManyInitRequests_ClosesTheConnection()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());

            await graphqlWsClient.StartConnectionAsync();

            // a response to the first message should have been transmitted
            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, GqltwsConstants.CustomCloseEventIds.TooManyInitializationRequests);

            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task NotSendingTheInitMessageWithinTimeoutPeriod_ClosesTheConnection()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            await graphqlWsClient.StartConnectionAsync(initializationTimeout: TimeSpan.FromMilliseconds(5));

            // no response should ever have been given
            // and the connection should have been closed from the server
            connection.AssertServerClosedConnection();

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, GqltwsConstants.CustomCloseEventIds.ConnectionInitializationTimeout);
        }

        [Test]
        public async Task WhenPingReceived_PongIsSent()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsPingMessage());
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnectionAsync();

            // no response should ever have been given
            // and the connection should have been closed from the server
            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.PONG);
            connection.AssertClientClosedConnection();

            // no other messages
            Assert.AreEqual(0, connection.ResponseMessageCount);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task WhenClientSendsSubscriptionComplete_ServerDropsTheSubscription()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // queue the subscription
            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsClientSubscribeMessage()
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
            connection.QueueClientMessage(new GqltwsSubscriptionCompleteMessage("abc"));

            // ensure it was removed
            connection.QueueAction(() =>
            {
                Assert.AreEqual(0, graphqlWsClient.Subscriptions.Count());
            });

            // close out
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertClientClosedConnection();

            Assert.AreEqual(0, graphqlWsClient.Subscriptions.Count());
            Assert.AreEqual(0, connection.ResponseMessageCount);

            // the connection should be closed in response to the second message
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, (int)ConnectionCloseStatus.NormalClosure);
        }

        [Test]
        public async Task RecievingAnInvaidMessageType_ServerClosesConnectionImmediately()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new FakeGqltwsMessage());

            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertServerClosedConnection();

            Assert.AreEqual(0, connection.ResponseMessageCount);

            // the connection should be closed in response to the second message
            // with a specific code
            Assert.IsTrue(connection.CloseStatus.HasValue);
            Assert.AreEqual((int)connection.CloseStatus.Value, (int)GqltwsConstants.CustomCloseEventIds.InvalidMessageType);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task RecievedPingMessages_ResponsesWithAPong()
        {
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsPingMessage());
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.PONG);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StopSubscription_AgainstNonExistantId_IsIgnored()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());

            // mimic the client sending a complete message for a subscription
            // not currently registered
            connection.QueueClientMessage(new GqltwsSubscriptionCompleteMessage()
            {
                Id = "abc123",
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            Assert.AreEqual(0, connection.ResponseMessageCount);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ReceiveEvent_SubscriptionsElectsToCloseOnStartedSubscription_YieldsNEXTMessage_AndCompleteMessage()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObjectAndComplete { property1 } } }",
                },
            };

            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessMessageAsync(startMessage);

            // mimic new data for the registered subscription being processed by some
            // other mutation
            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GqltwsSubscriptionController.WatchForPropObjectAndComplete),
                SchemaTypeName = new GraphSchema().FullyQualifiedSchemaTypeName(),
            };

            await graphqlWsClient.ReceiveEventAsync(evt);

            // the connection should receive a data package
            connection.AssertGqltwsResponse(
                GqltwsMessageType.NEXT,
                "abc",
                @"{
                    ""data"" : {
                        ""gqltwsSubscription"" : {
                            ""watchForPropObjectAndComplete"" : {
                                ""property1"" : ""value1"",
                            }
                        }
                    }
                }");

            // the connection should receive the complete message
            connection.AssertGqltwsResponse(GqltwsMessageType.COMPLETE, "abc");

            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ReceiveEvent_SubscriptionsElectsSkipAndCompleteOnStartedSubscription_YieldsJustCompleteMessage()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            var startMessage = new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObjectSkipAndComplete { property1 } } }",
                },
            };

            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessMessageAsync(startMessage);

            // mimic new data for the registered subscription being processed by some
            // other mutation
            var evt = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                DataTypeName = typeof(TwoPropertyObject).Name,
                Data = new TwoPropertyObject()
                {
                    Property1 = "value1",
                    Property2 = 33,
                },
                EventName = nameof(GqltwsSubscriptionController.WatchForPropObjectSkipAndComplete),
                SchemaTypeName = new GraphSchema().FullyQualifiedSchemaTypeName(),
            };

            await graphqlWsClient.ReceiveEventAsync(evt);

            // the connection should NOT receive a data package
            // the connection should receive the complete message
            connection.AssertGqltwsResponse(GqltwsMessageType.COMPLETE, "abc");

            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task Deserialize_InvalidMessage_ProcessesUnknownMessage()
        {
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage(new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage(new GqltwsPingMessage());
            connection.QueueClientMessage(new MockSocketMessage("{invalid-json-payload"));
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnectionAsync();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.PONG);

            // server closes with an error status
            connection.AssertServerClosedConnection((ConnectionCloseStatus)GqltwsConstants.CustomCloseEventIds.InvalidMessageType);
            graphqlWsClient.Dispose();
        }
    }
}