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
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages;
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
        private async Task<(MockClientConnection, GQLWSClientProxy<GraphSchema>)>
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
            var subServer = server.ServiceProvider.GetService<ISubscriptionServer<GraphSchema>>();

            var graphqlWsCLient = await subServer.RegisterNewClient(socketClient);
            return (socketClient, graphqlWsCLient as GQLWSClientProxy<GraphSchema>);
        }

        [Test]
        public async Task GeneralPropertyCheck()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

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

            (var socketClient, var graphqlWsClient) = await this.CreateConnection();
            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            graphqlWsClient.ConnectionOpening += ConnectionOpening;
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Opening Event Handler not called");
        }

        [Test]
        public async Task WhenConnectionCloses_EventFires()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            bool eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            graphqlWsClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            socketClient.QueueConnectionCloseMessage();
            await graphqlWsClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the graphql-ws client to do the close
            var socketClient = new MockClientConnection(autoCloseOnReadCloseMessage: false);
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var graphqlWsClient = new GQLWSClientProxy<GraphSchema>(
                socketClient,
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
            socketClient.QueueConnectionCloseMessage();
            await graphqlWsClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueConnectionCloseMessage();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.AreEqual(0, socketClient.QueuedMessageCount);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
           {
               await graphqlWsClient.StartConnection();
           });
        }

        [Test]
        public async Task StartConnection_ContinuesToReadMessagesFromTheSocketConnect_UntilCloseMessage()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueConnectionCloseMessage();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            Assert.AreEqual(0, socketClient.QueuedMessageCount);
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();

            var routesAdded = 0;
            var routesRemoved = 0;
            void RouteAdded(object o, GQLWSSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesAdded += 1;
            }

            void RouteRemoved(object o, GQLWSSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesRemoved += 1;
            }

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, Enumerable.Count<ISubscription<GraphSchema>>(graphqlWsClient.Subscriptions));
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
        public async Task StartSubscription_ButMessageIsAQuery_YieldsDataMessage()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            // use start message to send a query, not a subscription request
            // client should respond with expected data
            // then a complete
            var startMessage = new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query {  fastQuery { property1 } }",
                },
            };

            await graphqlWsClient.DispatchMessage(startMessage);
            socketClient.AssertGQLWSResponse(
                  GQLWSMessageType.DATA,
                  "abc",
                  @"{
                    ""data"" : {
                        ""fastQuery"" : {
                            ""property1"" : ""bob""
                        }
                    }
                }");

            socketClient.AssertGQLWSResponse(GQLWSMessageType.COMPLETE);
        }

        [Test]
        public async Task ReceiveEvent_OnStartedSubscription_YieldsDataMessage()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            var startMessage = new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            };

            await graphqlWsClient.DispatchMessage(startMessage);

            var route = new SchemaItemPath("[subscription]/GQLWSSubscription/WatchForPropObject");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            socketClient.AssertGQLWSResponse(
                GQLWSMessageType.DATA,
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
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            var route = new SchemaItemPath("[subscription]/GQLWSSubscription/WatchForPropObject");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task ReceiveEvent_OnNonSubscribedEventNAme_YieldsNothing()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            // start a real subscription so the client is tracking one
            var startMessage = new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            };

            await graphqlWsClient.DispatchMessage(startMessage);

            // fire an event against a route not tracked, ensure the client skips it.
            var route = new SchemaItemPath("[subscription]/GQLWSSubscription/WatchForPropObject_NotReal");
            await graphqlWsClient.ReceiveEvent(route, new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 33,
            });

            Assert.AreEqual(0, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task StopSubscription_RemovesSubscriptionCorrectly()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueClientMessage(new GQLWSClientStopMessage()
            {
                Id = "abc",
            });

            socketClient.QueueConnectionCloseMessage();

            var routesAdded = 0;
            var routesRemoved = 0;
            void RouteAdded(object o, GQLWSSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesAdded += 1;
            }

            void RouteRemoved(object o, GQLWSSubscriptionFieldEventArgs e)
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
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientStopMessage()
            {
                Id = "abc123",
            });

            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();

            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc1",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();

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
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  gQLWSSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();

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
            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }

        [Test]
        public async Task SendConnectionTerminate_ClosesConnectionFromServer()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientConnectionTerminateMessage());

            var eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            graphqlWsClient.ConnectionClosed += ConnectionClosed;

            await graphqlWsClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task InvalidMessageType_ResultsInError()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage((GQLWSMessage)new FakeGQLWSMessage()
            {
                Type = "invalid_type",
            });

            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await graphqlWsClient.StartConnection();
            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.ERROR);
        }

        [Test]
        public async Task SendMessage_AsInterface_MessageIsDeliveredToConnection()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();
            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            var client = graphqlWsClient as ISubscriptionClientProxy;
            await client.SendMessage(new GQLWSServerAckOperationMessage());

            Assert.AreEqual(1, socketClient.ResponseMessageCount);
        }

        [Test]
        public async Task SendMessage_AsInterface_WithNonGQLWSMessage_ThrowsException()
        {
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();
            socketClient.QueueConnectionCloseMessage();

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
            (var socketClient, var graphqlWsClient) = await this.CreateConnection();
            socketClient.QueueClientMessage(new GQLWSClientConnectionInitMessage());
            socketClient.QueueClientMessage(new GQLWSClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "query { fastQuery { property1 } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();
            await graphqlWsClient.StartConnection();

            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_ACK);
            socketClient.AssertGQLWSResponse(GQLWSMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertGQLWSResponse(
                GQLWSMessageType.DATA,
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