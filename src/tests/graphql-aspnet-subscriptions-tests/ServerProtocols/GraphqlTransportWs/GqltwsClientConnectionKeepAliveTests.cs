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
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GqltwsClientConnectionKeepAliveTests
    {
        internal class FakeClient : GqltwsClientProxy
        {
            public FakeClient(ClientConnectionState state)
            {
                this.State = state;
                this.Messages = new List<GqltwsMessage>();
            }

            public override Task SendMessage(GqltwsMessage message)
            {
                this.Messages.Add(message);
                return Task.CompletedTask;
            }

            public List<GqltwsMessage> Messages { get; }

            public override ClientConnectionState State { get; }
        }

        [Test]
        public async Task AppropriateKeepAlivesAreSentWhenActive()
        {
            var proxy = new FakeClient(ClientConnectionState.Open);

            using var monitor = new GqltwsClientConnectionKeepAliveMonitor(
                proxy,
                TimeSpan.FromMilliseconds(40));

            monitor.Start();
            await Task.Delay(110);
            monitor.Stop();

            // ensure some messages were written
            Assert.IsTrue(proxy.Messages.Count > 0);

            // ensure that they are all ping messages
            Assert.AreEqual(proxy.Messages.Count, proxy.Messages.Cast<GqltwsPingMessage>().Count());
        }

        [Test]
        public async Task KeepAlives_ForClosedClient_SendsNoMessages()
        {
            var proxy = new FakeClient(ClientConnectionState.Closed);

            using var monitor = new GqltwsClientConnectionKeepAliveMonitor(
                proxy,
                TimeSpan.FromMilliseconds(40));
            monitor.Start();
            await Task.Delay(110);
            monitor.Stop();

            // ensure that they are all ping messages
            Assert.AreEqual(0, proxy.Messages.Count);
        }
    }
}