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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Apollo.Messages;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Apollo.ApolloTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Internal;

    [TestFixture]
    public class ApolloSubscriptionServerTests
    {
        [Test]
        public async Task WhenConnectionEstablished_RequiredMessagesReturned()
        {
            var socketClient = new MockClientConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(
                provider,
                null,
                socketClient,
                options,
                new ApolloMessageConverterFactory(),
                false);

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                options,
                new Mock<ISubscriptionEventListener>().Object);

            subscriptionServer.RegisterNewClient(apolloClient);

            var message = new ApolloClientConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new MockClientMessage(new ApolloClientConnectionInitMessage()));
            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await apolloClient.StartConnection();

            // the server should have sent back two messages to the client (ack and keep alive) per the protocol
            Assert.AreEqual(2, socketClient.ResponseMessageCount);

            // ensure the two response messages are of the appropriate type
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_ACK, true);
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_KEEP_ALIVE, true);
        }

        [Test]
        public async Task WhenSubscriptionStarted_GeneratesNewSubscriptionRegistration()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ApolloSubscriptionController>()
                .AddSubscriptionServer()
                .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var message = new ApolloClientConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueNewSubscription(
                "abc123",
                "subscription { apolloSubscription { watchForPropObject { property1 property2} } }");

            socketClient.QueueConnectionCloseMessage();

            bool eventTriggered = false;

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                testServer.Schema,
                testServer.RetrieveSubscriptionServerOptions(),
                new Mock<ISubscriptionEventListener>().Object);

            subscriptionServer.SubscriptionRegistered += (sender, args) =>
            {
                eventTriggered = true;
                Assert.AreEqual(1, subscriptionServer.Subscriptions.RetrieveSubscriptions(apolloClient).Count());

                var route = new GraphFieldPath("[subscription]/ApolloSubscription/WatchForPropObject");
                Assert.AreEqual(1, subscriptionServer.Subscriptions.RetrieveSubscriptions(route).Count());
            };

            subscriptionServer.RegisterNewClient(apolloClient);

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.IsTrue(eventTriggered, "NewSub event never fired");
        }

        [Test]
        public async Task WhenSubscriptionStoped_SubscriptionRegistrationIsDropped()
        {
            var testServer = new TestServerBuilder()
              .AddGraphController<ApolloSubscriptionController>()
              .AddSubscriptionServer()
              .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var message = new ApolloClientConnectionInitMessage();

            // queue a message sequence to the server
            socketClient.QueueClientMessage(new ApolloClientConnectionInitMessage());
            socketClient.QueueNewSubscription(
                "abc123",
                "subscription { apolloSubscription { watchForPropObject { property1 property2} } }");

            socketClient.QueueClientMessage(new ApolloClientStopMessage("abc123"));
            socketClient.QueueConnectionCloseMessage();

            bool subscribeEventFired = false;
            bool unsubscribeEventFired = false;

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                testServer.Schema,
                testServer.RetrieveSubscriptionServerOptions(),
                new Mock<ISubscriptionEventListener>().Object);

            subscriptionServer.SubscriptionRegistered += (sender, args) =>
            {
                subscribeEventFired = true;
            };

            subscriptionServer.SubscriptionRemoved += (sender, args) =>
            {
                unsubscribeEventFired = true;
                Assert.AreEqual(0, subscriptionServer.Subscriptions.RetrieveSubscriptions(apolloClient).Count());

                var route = new GraphFieldPath("[subscription]/ApolloSubscription/WatchForPropObject");
                Assert.AreEqual(0, subscriptionServer.Subscriptions.RetrieveSubscriptions(route).Count());
            };

            subscriptionServer.RegisterNewClient(apolloClient);
            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            await apolloClient.StartConnection();
            Assert.IsTrue(subscribeEventFired, "Subscribe event never fired");
            Assert.IsTrue(unsubscribeEventFired, "Unsubscribe never fired");
        }

        [Test]
        public async Task RecieveEvent_IsCommunicatedToTheSubscriber()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ApolloSubscriptionController>()
                .AddSubscriptionServer()
                .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var message = new ApolloClientConnectionInitMessage();

            var data = new GraphQueryData()
            {
                Query = "subscription { apolloSubscription { watchForPropObject { property1 property2} } }",
            };

            var queryPlan = await testServer.CreateQueryPlan(data.Query);

            var provider = testServer.ServiceProvider.CreateScope();
            var mockConnection = new Mock<ISubscriptionClientProxy<GraphSchema>>();
            mockConnection.Setup(x => x.ServiceProvider).Returns(provider.ServiceProvider);
            mockConnection.Setup(x => x.SendMessage(It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            var subscription = new ClientSubscription<GraphSchema>(
                mockConnection.Object,
                data,
                queryPlan,
                queryPlan.Operations.First().Value,
                "abc123");

            var subscriptionServer = new ApolloSubscriptionServer<GraphSchema>(
                testServer.Schema,
                testServer.RetrieveSubscriptionServerOptions(),
                new Mock<ISubscriptionEventListener>().Object);

            await subscriptionServer.AddSubscription(subscription);
            await subscriptionServer.ReceiveEvent(new SubscriptionEvent()
            {
                Data = new TwoPropertyObject()
                {
                    Property1 = "prop1",
                    Property2 = 55,
                },
                DataTypeName = typeof(TwoPropertyObject).FullName,
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "[subscription]/ApolloSubscription/WatchForPropObject",
            });

            mockConnection.Verify(x => x.SendMessage(It.IsAny<object>()), "No Message was sent to the connection");
        }

        [Test]
        public async Task WhenAClientRegistersASubscriptionId_ThatIsAlreadyRegistered_AnErrorIsReturned()
        {
            var testServer = new TestServerBuilder()
               .AddGraphController<ApolloSubscriptionController>()
               .AddSubscriptionServer()
               .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var initMessage = new ApolloClientConnectionInitMessage();
            socketClient.QueueClientMessage(initMessage);
            var startMessage = new ApolloClientStartMessage()
            {
                Id = "abc123",
                Payload = new GraphQueryData()
                {
                    Query = "subscription { apolloSubscription { watchForPropObject { property1 property2} } }",
                },
            };

            // queue a start of the same subscription twice
            socketClient.QueueClientMessage(startMessage);
            socketClient.QueueClientMessage(startMessage);
            socketClient.QueueConnectionCloseMessage();

            var subServer = testServer.RetrieveSubscriptionServer();
            subServer.RegisterNewClient(apolloClient);

            // the client should receive, in order
            // init => ack, KA
            // startSub => (nothing)
            // startSub => (error, id in use)
            // terminate => (nothing)
            await apolloClient.StartConnection();
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_ACK);
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertApolloResponse(ApolloMessageType.ERROR, "abc123", true);
        }

        [Test]
        public async Task SyntaxErrorInSubscriptionQuery_ResultsInErrorResponse()
        {
            var testServer = new TestServerBuilder()
               .AddGraphController<ApolloSubscriptionController>()
               .AddSubscriptionServer()
               .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var initMessage = new ApolloClientConnectionInitMessage();
            socketClient.QueueClientMessage(initMessage);

            // invalid subscription query, missing {
            var startMessage = new ApolloClientStartMessage()
            {
                Id = "abc123",
                Payload = new GraphQueryData()
                {
                    Query = "subscription apolloSubscription { watchForPropObject { property1 property2} } }",
                },
            };

            socketClient.QueueClientMessage(startMessage);
            socketClient.QueueConnectionCloseMessage();

            var subServer = testServer.RetrieveSubscriptionServer();
            subServer.RegisterNewClient(apolloClient);

            // the client should receive, in order
            // init => ack, KA
            // startSub => error, complete
            await apolloClient.StartConnection();
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_ACK);
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertApolloResponse(ApolloMessageType.ERROR, "abc123");
            socketClient.AssertApolloResponse(ApolloMessageType.COMPLETE, "abc123");
        }

        [Test]
        public async Task InvalidFieldSet_ResultsInDataResponse_withMultipleErrors()
        {
            var testServer = new TestServerBuilder()
               .AddGraphController<ApolloSubscriptionController>()
               .AddSubscriptionServer()
               .Build();

            (var socketClient, var apolloClient) = testServer.CreateSubscriptionClient();

            var initMessage = new ApolloClientConnectionInitMessage();
            socketClient.QueueClientMessage(initMessage);

            // invalid property names
            var startMessage = new ApolloClientStartMessage()
            {
                Id = "abc123",
                Payload = new GraphQueryData()
                {
                    Query = "subscription { apolloSubscription { watchForPropObject { fakeProperty1 fakeProperty2} } }",
                },
            };

            socketClient.QueueClientMessage(startMessage);
            socketClient.QueueConnectionCloseMessage();

            var subServer = testServer.RetrieveSubscriptionServer();
            subServer.RegisterNewClient(apolloClient);

            var expectedJsonpayload = @"
            {
                ""errors"" : [
                    {
                        ""message"" : ""The graph type 'TwoPropertyObject' does not contain a field named 'fakeProperty1'."",
                        ""locations"" : [{""line"" : 1,""column"" : 58}],
                        ""extensions"" : {
                            ""code"" : ""INVALID_DOCUMENT"" ,
                            ""timestamp"" : ""<anyValue>"" ,
                            ""severity"" : ""CRITICAL"" ,
                            ""metaData"" : {
                                    ""Rule"" : ""5.3.1"" ,
                                    ""RuleReference"" : ""<anyValue>"" }
                                }
                        },
                    {
                        ""message"" : ""The graph type 'TwoPropertyObject' does not contain a field named 'fakeProperty2'."",
                        ""locations"" : [{""line"" : 1,""column"" : 72}],
                        ""extensions"" : {
                            ""code"" : ""INVALID_DOCUMENT"" ,
                            ""timestamp"" : ""<anyValue>"" ,
                            ""severity"" : ""CRITICAL"" ,
                            ""metaData"" : {
                                ""Rule"" : ""5.3.1"" ,
                                ""RuleReference"" : ""<anyValue>"" }
                            }
                    }]
            }
";

            // the client should receive, in order
            // init => ack, KA
            // startSub => data (with errors), complete
            await apolloClient.StartConnection();
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_ACK);
            socketClient.AssertApolloResponse(ApolloMessageType.CONNECTION_KEEP_ALIVE);
            socketClient.AssertApolloResponse(ApolloMessageType.DATA,  "abc123", expectedJsonpayload);
            socketClient.AssertApolloResponse(ApolloMessageType.COMPLETE, "abc123");
        }
    }
}