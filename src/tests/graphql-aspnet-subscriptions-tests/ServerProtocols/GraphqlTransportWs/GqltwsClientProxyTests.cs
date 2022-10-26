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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Mocks;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public partial class GqltwsClientProxyTests
    {
        private (MockClientConnection, GqltwsClientProxy<GraphSchema>, Mock<ISubscriptionEventRouter>) CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GqltwsSubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromMinutes(15);
                    options.AuthenticatedRequestsOnly = false;
                })
                .Build();

            var router = new Mock<ISubscriptionEventRouter>();

            var connection = server.CreateClientConnection(GqltwsConstants.PROTOCOL_NAME);
            var serverOptions = server.ServiceProvider.GetRequiredService<SubscriptionServerOptions<GraphSchema>>();

            var subClient = new GqltwsClientProxy<GraphSchema>(
                connection,
                server.Schema,
                router.Object);
            return (connection, subClient, router);
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            Assert.IsNotNull(string.IsNullOrWhiteSpace(graphqlWsClient.Id));
            Assert.AreNotEqual(Guid.Empty.ToString(), graphqlWsClient.Id);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the graphql-ws client to do the close
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();

            // execute the connection sequence
            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnection();

            connection.AssertClientClosedConnection();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // init a connection then close the socket
            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueConnectionClosedByClient();

            Assert.AreEqual(2, connection.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.AreEqual(0, connection.QueuedMessageCount);

            // attempt to restart the closed connection
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await graphqlWsClient.StartConnection();
            });

            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartConnection_ContinuesToReadMessagesFromTheSocketConnect_UntilCloseMessage()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueConnectionClosedByClient(); // socket level close message

            Assert.AreEqual(2, connection.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.AreEqual(0, connection.QueuedMessageCount);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            // startup the connection then register a subscription
            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            router.Verify(x => x.AddReceiver(graphqlWsClient, It.IsAny<SubscriptionEventName>()), Times.Once());
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_ImmediatelyYieldsNEXTMessage()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
            await graphqlWsClient.ProcessMessage(startMessage);

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
            await graphqlWsClient.ProcessMessage(startMessage);

            connection.AssertGqltwsResponse(GqltwsMessageType.ERROR, "abc");
            connection.AssertConnectionIsOpen();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ReceiveEvent_OnStartedSubscription_YieldsNEXTMessage()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
            await graphqlWsClient.ProcessMessage(startMessage);

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

            await graphqlWsClient.ReceiveEvent(evt);

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
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

            await graphqlWsClient.ReceiveEvent(evt);

            // nothing should have been sent to the connection
            Assert.AreEqual(0, connection.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventNAme_YieldsNothing()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
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

            await graphqlWsClient.ProcessMessage(startMessage);

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueClientMessage((object)new GqltwsSubscriptionCompleteMessage()
            {
                Id = "abc",
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc1",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gqltwsSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.ERROR);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();
            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query { fastQuery { property1 } }",
                },
            });

            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnection();

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());

            await graphqlWsClient.StartConnection();

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            await graphqlWsClient.StartConnection(initializationTimeout: TimeSpan.FromMilliseconds(5));

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsPingMessage());
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnection();

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new FakeGqltwsMessage());

            await graphqlWsClient.StartConnection();

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

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsPingMessage());
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnection();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.PONG);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task StopSubscription_AgainstNonExistantId_IsIgnored()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            (var connection, var graphqlWsClient, var router) = this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());

            // mimic the client sending a complete message for a subscription
            // not currently registered
            connection.QueueClientMessage((object)new GqltwsSubscriptionCompleteMessage()
            {
                Id = "abc123",
            });

            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            Assert.AreEqual(0, connection.ResponseMessageCount);
            graphqlWsClient.Dispose();
        }

        [Test]
        public async Task ReceiveEvent_SubscriptionsElectsToCloseOnStartedSubscription_YieldsNEXTMessage_AndCompleteMessage()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
            await graphqlWsClient.ProcessMessage(startMessage);

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

            await graphqlWsClient.ReceiveEvent(evt);

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
            await graphqlWsClient.ProcessMessage(startMessage);

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

            await graphqlWsClient.ReceiveEvent(evt);

            // the connection should NOT receive a data package
            // the connection should receive the complete message
            connection.AssertGqltwsResponse(GqltwsMessageType.COMPLETE, "abc");

            graphqlWsClient.Dispose();
        }
    }
}