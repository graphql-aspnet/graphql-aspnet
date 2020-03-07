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
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
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
            var apolloClient = new ApolloClientProxy<GraphSchema>(
                socketClient,
                options,
                new ApolloMessageConverterFactory(),
                false);

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
            var apolloClient = new ApolloClientProxy<GraphSchema>(
                socketClient,
                options,
                new ApolloMessageConverterFactory(),
                false);

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
    }
}