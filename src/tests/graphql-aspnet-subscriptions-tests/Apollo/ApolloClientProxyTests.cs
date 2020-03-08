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
    using System.Linq;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.Subscriptions.Tests.Apollo.ApolloTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Internal.Execution;

    [TestFixture]
    public partial class ApolloClientProxyTests
    {
        private async Task<(MockClientConnection, ApolloClientProxy<GraphSchema>)>
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
            var subServer = server.ServiceProvider.GetService<ISubscriptionServer<GraphSchema>>();

            var apolloClient = await subServer.RegisterNewClient(socketClient);
            return (socketClient, apolloClient as ApolloClientProxy<GraphSchema>);
        }

        [Test]
        public async Task GeneralPropertyCheck()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            Assert.IsNotNull(string.IsNullOrWhiteSpace(apolloClient.Id));
            Assert.AreNotEqual(Guid.Empty.ToString(), apolloClient.Id);
        }

        [Test]
        public async Task WhenConnectionOpened_EventFires()
        {
            bool eventCalled = false;
            void ConnectionOpening(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            (var socketClient, var apolloClient) = await this.CreateConnection();
            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            apolloClient.ConnectionOpening += ConnectionOpening;
            await apolloClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Opening Event Handler not called");
        }

        [Test]
        public async Task WhenConnectionCloses_EventFires()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            bool eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            apolloClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            socketClient.QueueConnectionCloseMessage();
            await apolloClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task StartConnection_OnReadClose_IfConnectionIsOpen_CloseConnection()
        {
            // set the underlying connection to not auto close when it retrieves a close message
            // from the queue, leaving it up to the apollo client to do the close
            var socketClient = new MockClientConnection(autoCloseOnReadCloseMessage: false);
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(
                socketClient,
                options,
                new ApolloMessageConverterFactory(),
                false);

            var eventCalled = false;
            void ConnectionClosed(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            apolloClient.ConnectionClosed += ConnectionClosed;

            // execute the connection sequence
            socketClient.QueueConnectionCloseMessage();
            await apolloClient.StartConnection();
            Assert.IsTrue(eventCalled, "Connection Closed Event Handler not called");
        }

        [Test]
        public async Task AttemptingToStartAClosedConnection_ThrowsException()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueConnectionCloseMessage();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await apolloClient.StartConnection();

            Assert.AreEqual(0, socketClient.QueuedMessageCount);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
           {
               await apolloClient.StartConnection();
           });
        }

        [Test]
        public async Task StartConnection_ContinuesToReadMessagesFromTheSocketConnect_UntilCloseMessage()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueConnectionCloseMessage();

            Assert.AreEqual(2, socketClient.QueuedMessageCount);

            // execute the connection sequence
            await apolloClient.StartConnection();

            Assert.AreEqual(0, socketClient.QueuedMessageCount);
        }

        [Test]
        public async Task StartSubscription_RegistersSubscriptionCorrectly()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();

            var routesAdded = 0;
            var routesRemoved = 0;
            void RouteAdded(object o, ApolloSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesAdded += 1;
            }

            void RouteRemoved(object o, ApolloSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesRemoved += 1;
            }

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, apolloClient.Subscriptions.Count());
            }

            apolloClient.SubscriptionRouteAdded += RouteAdded;
            apolloClient.SubscriptionRouteRemoved += RouteRemoved;
            apolloClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.AreEqual(1, routesAdded);
            Assert.AreEqual(1, routesRemoved);
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task StopSubscription_RemovesSubscriptionCorrectly()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueClientMessage(new ApolloClientStopMessage()
            {
                Id = "abc",
            });

            socketClient.QueueConnectionCloseMessage();

            var routesAdded = 0;
            var routesRemoved = 0;
            void RouteAdded(object o, ApolloSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesAdded += 1;
            }

            void RouteRemoved(object o, ApolloSubscriptionFieldEventArgs e)
            {
                if (e.Field.Name == "watchForPropObject")
                    routesRemoved += 1;
            }

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;

                // should be no subscriptions by the time the connection closes
                Assert.AreEqual(0, apolloClient.Subscriptions.Count());
            }

            apolloClient.SubscriptionRouteAdded += RouteAdded;
            apolloClient.SubscriptionRouteRemoved += RouteRemoved;
            apolloClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.AreEqual(1, routesAdded);
            Assert.AreEqual(1, routesRemoved);
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task StartMultipleSubscriptions_AllRegistered_ButRouteEventOnlyRaisedOnce()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            });
            socketClient.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = "abc1",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(2, apolloClient.Subscriptions.Count());
            }

            apolloClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
        }

        [Test]
        public async Task AttemptToStartMultipleSubscriptionsWithSameId_ResultsInErrorMessageForSecond()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            });
            socketClient.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = "abc",
                Payload = new GraphQueryData()
                {
                    Query = "subscription {  apolloSubscription { watchForPropObject { property1 } } }",
                },
            });

            socketClient.QueueConnectionCloseMessage();

            bool closeCalled = false;
            void ConnectionClosing(object o, EventArgs e)
            {
                closeCalled = true;
                Assert.AreEqual(1, apolloClient.Subscriptions.Count());
            }

            apolloClient.ConnectionClosing += ConnectionClosing;

            // execute the connection sequence
            await apolloClient.StartConnection();

            Assert.IsTrue(closeCalled, "Connection closing never called to verify client state");
            socketClient.AssertApolloResponse(AspNet.Apollo.Messages.ApolloMessageType.CONNECTION_ACK);
            socketClient.AssertApolloResponse(AspNet.Apollo.Messages.ApolloMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertApolloResponse(AspNet.Apollo.Messages.ApolloMessageType.ERROR);
        }

        [Test]
        public async Task InvalidMessageType_ResultsInError()
        {
            (var socketClient, var apolloClient) = await this.CreateConnection();

            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueClientMessage(new FakeApolloMessage()
            {
                Type = "invalid_type",
            });

            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await apolloClient.StartConnection();
            socketClient.AssertApolloResponse(AspNet.Apollo.Messages.ApolloMessageType.CONNECTION_ACK);
            socketClient.AssertApolloResponse(AspNet.Apollo.Messages.ApolloMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertApolloResponse(AspNet.Apollo.Messages.ApolloMessageType.ERROR);
        }
    }
}