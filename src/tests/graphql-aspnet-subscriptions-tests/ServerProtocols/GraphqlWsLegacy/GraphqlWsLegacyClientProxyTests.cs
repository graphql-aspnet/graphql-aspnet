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
    using System.Threading.Tasks;
    using GraphQL.AspNet;
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
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlWsLegacy.GraphqlWsLegacyData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public partial class GraphqlWsLegacyClientProxyTests
    {
        private async Task<(MockClientConnection, GraphqlWsLegacyClientProxy<GraphSchema>)>
            CreateConnection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GraphqlWsLegacySubscriptionController>()
                .AddSubscriptionServer((options) =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromMinutes(15);
                })
                .Build();

            var socketClient = server.CreateClientConnection();
            var serverOptions = server.ServiceProvider.GetRequiredService<SubscriptionServerOptions<GraphSchema>>();
            var GraphqlWsLegacyClient = new GraphqlWsLegacyClientProxy<GraphSchema>(
                socketClient,
                GraphqlWsLegacyConstants.PROTOCOL_NAME);

            var subServer = server.ServiceProvider.GetService<ISubscriptionServer<GraphSchema>>();

            await subServer.RegisterNewClient(GraphqlWsLegacyClient);
            return (socketClient, GraphqlWsLegacyClient);
        }

        [Test]
        public async Task GeneralPropertyCheck()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            Assert.IsNotNull(string.IsNullOrWhiteSpace(GraphqlWsLegacyClient.Id));
            Assert.AreNotEqual(Guid.Empty.ToString(), GraphqlWsLegacyClient.Id);
        }

        [Test]
        public async Task WhenConnectionOpened_EventFires()
        {
            bool eventCalled = false;
            void ConnectionOpening(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();
            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            GraphqlWsLegacyClient.ConnectionOpening += ConnectionOpening;
            await GraphqlWsLegacyClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Opening Event Handler not called");
        }

        [Test]
        public async Task WhenConnectionCloses_EventFires()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            bool eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            GraphqlWsLegacyClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            socketClient.QueueConnectionClosedByClient();
            await GraphqlWsLegacyClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the GraphqlWsLegacy client to do the close
            var socketClient = new MockClientConnection(autoCloseOnReadCloseMessage: false);
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var GraphqlWsLegacyClient = new GraphqlWsLegacyClientProxy<GraphSchema>(
                socketClient,
                GraphqlWsLegacyConstants.PROTOCOL_NAME,
                null,
                false);

            var eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            GraphqlWsLegacyClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            socketClient.QueueConnectionClosedByClient();
            await GraphqlWsLegacyClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueConnectionClosedByClient();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection();

            Assert.AreEqual(0, socketClient.QueuedMessageCount);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
           {
               await GraphqlWsLegacyClient.StartConnection();
           });
        }

        [Test]
        public async Task StartConnection_ContinuesToReadMessagesFromTheSocketConnect_UntilCloseMessage()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueConnectionClosedByClient();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection();

            Assert.AreEqual(0, socketClient.QueuedMessageCount);
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

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
                Assert.AreEqual(1, GraphqlWsLegacyClient.Subscriptions.Count());
            }

            GraphqlWsLegacyClient.SubscriptionRouteAdded += RouteAdded;
            GraphqlWsLegacyClient.SubscriptionRouteRemoved += RouteRemoved;
            GraphqlWsLegacyClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection();
            Assert.AreEqual(1, routesAdded);
            Assert.AreEqual(1, routesRemoved);
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task StartSubscription_ButMessageIsAQuery_YieldsDataMessage()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

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
            await GraphqlWsLegacyClient.ProcessMessage(startMessage);
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
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            var startMessage = new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            };

            await socketClient.OpenAsync(GraphqlWsLegacyConstants.PROTOCOL_NAME);
            await GraphqlWsLegacyClient.ProcessMessage(startMessage);

            var route = new SchemaItemPath("[subscription]/GraphqlWsLegacySubscription/WatchForPropObject");
            await GraphqlWsLegacyClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

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
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            var route = new SchemaItemPath("[subscription]/GraphqlWsLegacySubscription/WatchForPropObject");
            await GraphqlWsLegacyClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventNAme_YieldsNothing()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            // start a real subscription so the client is tracking one
            var startMessage = new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            };

            await GraphqlWsLegacyClient.ProcessMessage(startMessage);

            // fire an event against a route not tracked, ensure the client skips it.
            var route = new SchemaItemPath("[subscription]/GraphqlWsLegacySubscription/WatchForPropObject_NotReal");
            await GraphqlWsLegacyClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task StopSubscription_RemovesSubscriptionCorrectly()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStopMessage()
            {
                Id = "abc",
            });

            socketClient.QueueConnectionClosedByClient();

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
                Assert.AreEqual(0, GraphqlWsLegacyClient.Subscriptions.Count());
            }

            GraphqlWsLegacyClient.SubscriptionRouteAdded += RouteAdded;
            GraphqlWsLegacyClient.SubscriptionRouteRemoved += RouteRemoved;
            GraphqlWsLegacyClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection();
            Assert.AreEqual(1, routesAdded);
            Assert.AreEqual(1, routesRemoved);
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task StopSubscription_AgainstNonExistantId_YieldsError()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientStopMessage()
            {
                Id = "abc123",
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection(TimeSpan.FromMilliseconds(30));

            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

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

            socketClient.QueueConnectionClosedByClient();

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(2, GraphqlWsLegacyClient.Subscriptions.Count());
            }

            GraphqlWsLegacyClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection(TimeSpan.FromSeconds(2));
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

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
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  graphqlWsLegacySubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionClosedByClient();

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, GraphqlWsLegacyClient.Subscriptions.Count());
            }

            GraphqlWsLegacyClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection(TimeSpan.FromSeconds(2));

            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
        }

        [Test]
        public async Task SendConnectionTerminate_ClosesConnectionFromServer()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionTerminateMessage());

            var eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            GraphqlWsLegacyClient.ConnectionClosed += ConnectionClosed;

            await GraphqlWsLegacyClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task InvalidMessageType_ResultsInError()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GraphqlWsLegacyClientConnectionInitMessage());
            socketClient.QueueClientMessage(new FakeGraphqlWsLegacyMessage()
            {
                Type = "invalid_type",
            });

            socketClient.QueueConnectionClosedByClient();

            // execute the connection sequence
            await GraphqlWsLegacyClient.StartConnection(TimeSpan.FromSeconds(2));
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_ACK);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGraphqlWsLegacyResponse(GraphqlWsLegacyMessageType.ERROR);
        }

        [Test]
        public async Task ExecuteQueryThroughStartMessage_YieldsQueryResult()
        {
            (var socketClient, var GraphqlWsLegacyClient) = await this.CreateConnection();
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
            await GraphqlWsLegacyClient.StartConnection(TimeSpan.FromSeconds(2));

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
        }
    }
}