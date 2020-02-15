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
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using NUnit.Framework.Internal.Execution;

    [TestFixture]
    public partial class ApolloClientProxyTests
    {
        [Test]
        public async Task WhenConnectionOpened_EventFires()
        {
            var socketClient = new MockClientConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

            bool eventCalled = false;
            void ConnectionOpening(object sender, EventArgs e)
            {
                eventCalled = true;
            }

            socketClient.QueueConnectionCloseMessage();

            // execute the connection sequence
            apolloClient.ConnectionOpening += ConnectionOpening;
            await apolloClient.StartConnection();

            Assert.IsTrue(eventCalled, "Connection Opening Event Handler not called");
        }

        [Test]
        public async Task WhenConnectionCloses_EventFires()
        {
            var socketClient = new MockClientConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

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
        public async Task WhenMessageRecieved_SingleAsyncDelegateIsActivated()
        {
            var socketClient = new MockClientConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

            bool delegateCalled = false;
            Task MessageRecieved(object sender, ApolloMessage message)
            {
                delegateCalled = true;
                return Task.CompletedTask;
            }

            // queue a message that would trigger the delegate
            socketClient.QueueClientMessage(new MockClientMessage(new ApolloSubscriptionStartMessage()));
            socketClient.QueueConnectionCloseMessage();

            apolloClient.RegisterAsyncronousMessageDelegate(MessageRecieved);

            // execute the connection sequence
            await apolloClient.StartConnection();

            Assert.IsTrue(delegateCalled, "Apollo message not transmitted to delegate");
        }

        [Test]
        public async Task WhenMessageRecieved_ThatIsntAnApolloMessage_UnknownMessageIsRaised()
        {
            var socketClient = new MockClientConnection();
            var options = new SubscriptionServerOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

            bool unknownMessageRecieved = false;
            Task MessageRecieved(object sender, ApolloMessage message)
            {
                unknownMessageRecieved = message is ApolloUnknownMessage;
                return Task.CompletedTask;
            }

            // queue a message that would trigger the delegate
            socketClient.QueueClientMessage(new MockClientMessage("non-apollo-message-payload"));
            socketClient.QueueConnectionCloseMessage();

            apolloClient.RegisterAsyncronousMessageDelegate(MessageRecieved);

            // execute the connection sequence
            await apolloClient.StartConnection();

            Assert.IsTrue(unknownMessageRecieved, "Invalid Apollo message payload not converted to proper unknown message type");
        }
    }
}