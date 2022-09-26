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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS.GraphQLWSData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public partial class GQLWSClientProxyTests
    {
        private async Task<(MockClientConnection, GQLWSClientProxy<GraphSchema>)> CreateConnection()
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
            connection.QueueConnectionCloseMessage();

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
            connection.QueueConnectionCloseMessage();
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
            connection.QueueConnectionCloseMessage();
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(closingCalled, "Connection Closing event not called");
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the graphql-ws client to do the close
            var connection = new MockClientConnection(autoCloseOnReadCloseMessage: false);
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var graphqlWsClient = new GQLWSClientProxy<GraphSchema>(
                connection,
                options,
                new GQLWSMessageConverterFactory(),
                null,
                false);

            var eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            graphqlWsClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            connection.QueueConnectionCloseMessage();
            await graphqlWsClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            // init a connection then close the socket
            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueConnectionCloseMessage();

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

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueConnectionCloseMessage(); // socket level close message

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
            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionCloseMessage();

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
            var startMessage = new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query {  fastQuery { property1 } }",
                },
            };

            await connection.OpenAsync(GQLWSConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessReceivedMessage(startMessage);

            connection.AssertGQLWSResponse(
                  GQLWSMessageType.NEXT,
                  "abc",
                  @"{
                    ""data"" : {
                        ""fastQuery"" : {
                            ""property1"" : ""bob""
                        }
                    }
                }");

            connection.AssertGQLWSResponse(GQLWSMessageType.COMPLETE);
        }

        [Test]
        public async Task ReceiveEvent_OnStartedSubscription_YieldsNEXTMessage()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            var startMessage = new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            };

            await connection.OpenAsync(GQLWSConstants.PROTOCOL_NAME);
            await graphqlWsClient.ProcessReceivedMessage(startMessage);

            // mimic new data for the registered subscription being processed by some
            // other mutation
            var route = new SchemaItemPath("[subscription]/GQLWSSubscription/WatchForPropObject");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            // the connection should receive a data package
            connection.AssertGQLWSResponse(
                GQLWSMessageType.NEXT,
                "abc",
                @"{
                    ""data"" : {
                        ""gQLWSSubscription"" : {
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
            var route = new SchemaItemPath("[subscription]/GQLWSSubscription/WatchForPropObject");
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
            var startMessage = new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            };

            await graphqlWsClient.ProcessReceivedMessage(startMessage);

            // fire an event against a route not tracked, ensure the client skips it.
            var route = new SchemaItemPath("[subscription]/GQLWSSubscription/WatchForPropObject_NotReal");
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

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueClientMessage(new GQLWSSubscriptionCompleteMessage()
            {
                Id = "abc",
            });

            connection.QueueConnectionCloseMessage();

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
                Assert.AreEqual(0, Enumerable.Count<ISubscription<GraphSchema>>(graphqlWsClient.Subscriptions));
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
        public async Task StopSubscription_AgainstNonExistantId_YieldsError()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());

            // mimic the client sending a complete message for a subscription
            // not currently registered
            connection.QueueClientMessage(new GQLWSSubscriptionCompleteMessage()
            {
                Id = "abc123",
            });

            connection.QueueConnectionCloseMessage();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            connection.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            connection.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc1",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionCloseMessage();

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(2, Enumerable.Count<ISubscription<GraphSchema>>(graphqlWsClient.Subscriptions));
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

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            connection.QueueConnectionCloseMessage();

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, Enumerable.Count<ISubscription<GraphSchema>>(graphqlWsClient.Subscriptions));
            }

            graphqlWsClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
            connection.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            connection.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }

        [Test]
        public async Task InvalidMessageType_ResultsInError()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage((GQLWSMessage)new FakeGQLWSMessage()
            {
                Type = "invalid_type",
            });

            connection.QueueConnectionCloseMessage();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            connection.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            connection.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }

        [Test]
        public async Task SendMessage_AsInterface_MessageIsDeliveredToConnection()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();
            connection.QueueConnectionCloseMessage();

            // execute the connection sequence
            var client = graphqlWsClient as ISubscriptionClientProxy;

            await connection.OpenAsync(GQLWSConstants.PROTOCOL_NAME);
            await client.SendMessage(new GQLWSServerAckOperationMessage());

            Assert.AreEqual(1, connection.ResponseMessageCount);
        }

        [Test]
        public async Task SendMessage_AsInterface_WithNonGQLWSMessage_ThrowsException()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();
            connection.QueueConnectionCloseMessage();

            // execute the connection sequence
            var client = graphqlWsClient as ISubscriptionClientProxy;
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await client.SendMessage(new object());
            });
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();
            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage(new GQLWSClientSubscribeMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query { fastQuery { property1 } }",
                },
            });

            connection.QueueConnectionCloseMessage();
            await graphqlWsClient.StartConnection();

            connection.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            connection.AssertGQLWSResponse(
                GQLWSMessageType.NEXT,
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
        public async Task RecievedPingMessages_ResponsesWithAPong()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            connection.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            connection.QueueClientMessage(new GQLWSPingMessage());
            connection.QueueConnectionCloseMessage();

            await graphqlWsClient.StartConnection();

            connection.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            connection.AssertGQLWSResponse(GQLWSMessageType.PONG);
        }

        [Test]
        public async Task SendingAErrorMessage_WriteAppropriateErrorToConnection()
        {
            (var connection, var graphqlWsClient) = await this.CreateConnection();

            var error = new GraphExecutionMessage(
                GraphMessageSeverity.Warning,
                "Error Message",
                "ERROR_CODE");

            await connection.OpenAsync(GQLWSConstants.PROTOCOL_NAME);
            await graphqlWsClient.SendErrorMessage(error);

            connection.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }
    }
}