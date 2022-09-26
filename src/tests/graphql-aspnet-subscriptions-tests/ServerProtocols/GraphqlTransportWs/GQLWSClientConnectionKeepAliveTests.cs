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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.BidirectionalMessages;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GQLWSClientConnectionKeepAliveTests
    {
        [Test]
        public async Task AppropriateKeepAlivesAreSentWhenActive()
        {
            var messages = new List<object>();
            var proxy = new Mock<ISubscriptionClientProxy>();
            proxy.Setup(x => x.State).Returns(ClientConnectionState.Open);

            proxy.Setup(x => x.SendMessage(It.IsAny<object>()))
                .Callback((object message) =>
                {
                    messages.Add(message);
                });

            using var monitor = new GQLWSClientConnectionKeepAliveMonitor(
                proxy.Object,
                TimeSpan.FromMilliseconds(40));
            monitor.Start();
            await Task.Delay(110);
            monitor.Stop();

            // ensure some messages were written
            Assert.IsTrue(messages.Count > 0);

            // ensure that they are all ping messages
            Assert.AreEqual(messages.Count, messages.Cast<GQLWSPingMessage>().Count());
        }

        [Test]
        public async Task KeepAlives_ForClosedClient_SendsNoMessages()
        {
            var messages = new List<object>();
            var proxy = new Mock<ISubscriptionClientProxy>();
            proxy.Setup(x => x.State).Returns(ClientConnectionState.Closed);

            proxy.Setup(x => x.SendMessage(It.IsAny<object>()))
                .Callback((object message) =>
                {
                    messages.Add(message);
                });

            using var monitor = new GQLWSClientConnectionKeepAliveMonitor(
                proxy.Object,
                TimeSpan.FromMilliseconds(40));
            monitor.Start();
            await Task.Delay(110);
            monitor.Stop();

            // ensure that they are all ping messages
            Assert.AreEqual(0, messages.Count);
        }
    }
}