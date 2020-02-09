// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscrptions.Tests.Apollo
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Schemas;
    using GraphQL.Subscrptions.Tests.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloClientProxyTests
    {
        [Test]
        public async Task WhenConnectionEstablished_RequiredMessagesReturned()
        {
            var socketClient = new MockClientConnection();
            var options = new SchemaSubscriptionOptions<GraphSchema>();

            var provider = new ServiceCollection().BuildServiceProvider();
            var apolloClient = new ApolloClientProxy<GraphSchema>(provider, null, socketClient, options, false);

            var supervisor = new ApolloClientSupervisor<GraphSchema>();
            supervisor.RegisterNewClient(apolloClient);

            var message = new ApolloConnectionInitMessage();

            socketClient.QueueClientMessage(new MockClientMessage(new ApolloConnectionInitMessage()));
            socketClient.QueueConnectionClose();

            await apolloClient.StartConnection();

            Assert.AreEqual(2, socketClient.MessagesSentToClient);
        }
    }
}