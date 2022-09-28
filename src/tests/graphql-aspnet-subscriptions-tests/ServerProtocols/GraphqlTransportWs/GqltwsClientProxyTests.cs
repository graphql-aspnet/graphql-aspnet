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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public partial class GqltwsClientProxyTests
    {
        private async Task<(MockClientConnection, GqltwsClientProxy<GraphSchema>)> CreateConnection()
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

            await subServer.RegisterNewClient(subClient);
            return (connection, subClient);
        }

        [Test]
        public async Task GeneralPropertyCheck()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            Assert.IsNotNull(string.IsNullOrWhiteSpace(graphqlWsClient.Id));
            Assert.AreNotEqual(Guid.Empty.ToString(), graphqlWsClient.Id);
        }

        [Test]
        public async Task WhenConnectionOpened_EventFires()
        {
            bool eventCalled = false;
            void ConnectionOpening(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            (var connection, var graphqlWsClient) = await this.CreateConnection();
            connection.QueueConnectionClosedByClient();

            // execute the connection sequence
            graphqlWsClient.ConnectionOpening += ConnectionOpening;
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Opening Event Handler not called");
        }

        [Test]
        public async Task WhenConnectionCloses_ClosedEventFires()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            bool closedCalled = false;

            void ConnectionClosed(object sender, EventArgs e)
            {
                closedCalled = true;
            }

            graphqlWsClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(closedCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task WhenConnectionCloses_ClosingEventFires()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            bool closingCalled = false;

            void ConnectionClosing(object sender, EventArgs e)
            {
                closingCalled = true;
            }

            graphqlWsClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(closingCalled, "Connection Closing event not called");
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the graphql-ws client to do the close
            var connection = new MockClientConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var graphqlWsClient = new GqltwsClientProxy<GraphSchema>(connection);

            var eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            graphqlWsClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            connection.QueueConnectionClosedByClient();
            await graphqlWsClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
        }

        [Test]
        public async Task StartConnection_ContinuesToReadMessagesFromTheSocketConnect_UntilCloseMessage()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueConnectionClosedByClient(); // socket level close message

            Assert.AreEqual(2, connection.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.AreEqual(0, connection.QueuedMessageCount);
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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

            var routesAdded = 0;
            var routesRemoved = 0;
            void RouteAdded(object o, SubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesAdded += 1;
            }

            void RouteRemoved(object o, SubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesRemoved += 1;
            }

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, graphqlWsClient.Subscriptions.Count());
            }

            graphqlWsClient.SubscriptionRouteAdded += RouteAdded;
            graphqlWsClient.SubscriptionRouteRemoved += RouteRemoved;
            graphqlWsClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.AreEqual(1, routesAdded);
            Assert.AreEqual(1, routesRemoved);
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_ImmediatelyYieldsNEXTMessage()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_WithSyntaxError_ImmediatelyYieldsError()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
        }

        [Test]
        public async Task ReceiveEvent_OnStartedSubscription_YieldsNEXTMessage()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
            var route = new SchemaItemPath("[subscription]/GqltwsSubscription/WatchForPropObject");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

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
        }

        [Test]
        public async Task ReceiveEvent_WhenNoSubscriptions_YieldsNothing()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            // no active subscriptions for the client, but a server event is received
            var route = new SchemaItemPath("[subscription]/GqltwsSubscription/WatchForPropObject");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            // nothing should have been sent to the connection
            Assert.AreEqual(0, connection.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventNAme_YieldsNothing()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
            var route = new SchemaItemPath("[subscription]/GqltwsSubscription/WatchForPropObject_NotReal");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            Assert.AreEqual(0, connection.ResponseMessageCount);
        }

        [Test]
        public async Task StopSubscription_RemovesSubscriptionCorrectly()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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

            var routesAdded = 0;
            var routesRemoved = 0;
            void RouteAdded(object o, SubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesAdded += 1;
            }

            void RouteRemoved(object o, SubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesRemoved += 1;
            }

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;

                // should be no subscriptions by the time the connection closes
                Assert.AreEqual(0, graphqlWsClient.Subscriptions.Count());
            }

            graphqlWsClient.SubscriptionRouteAdded += RouteAdded;
            graphqlWsClient.SubscriptionRouteRemoved += RouteRemoved;
            graphqlWsClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await graphqlWsClient.StartConnection();
            Assert.AreEqual(1, routesAdded);
            Assert.AreEqual(1, routesRemoved);
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(2, graphqlWsClient.Subscriptions.Count());
            }

            graphqlWsClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await graphqlWsClient.StartConnection();
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, graphqlWsClient.Subscriptions.Count());
            }

            graphqlWsClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.ERROR);
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();
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
        }

        [Test]
        public async Task SendingAErrorMessage_NoMessageSent()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            var error = new GraphExecutionMessage(
                GraphMessageSeverity.Warning,
                "Error Message",
                "ERROR_CODE");

            await connection.OpenAsync(GqltwsConstants.PROTOCOL_NAME);
            await graphqlWsClient.SendErrorMessage(error);

            Assert.AreEqual(0, connection.QueuedMessageCount);
        }

        [Test]
        public async Task SendingTooManyInitRequests_ClosesTheConnection()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
        }

        [Test]
        public async Task WhenClientSendsSubscriptionComplete_ServerDropsTheSubscription()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
        }

        [Test]
        public async Task RecievedPingMessages_ResponsesWithAPong()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage((object)new GqltwsClientConnectionInitMessage());
            connection.QueueClientMessage((object)new GqltwsPingMessage());
            connection.QueueConnectionClosedByClient();

            await graphqlWsClient.StartConnection();

            connection.AssertGqltwsResponse(GqltwsMessageType.CONNECTION_ACK);
            connection.AssertGqltwsResponse(GqltwsMessageType.PONG);
        }

        [Test]
        public async Task StopSubscription_AgainstNonExistantId_IsIgnored()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

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
        }
    }
}