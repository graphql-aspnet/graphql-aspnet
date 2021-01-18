// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Connections
{
    using System.Net.WebSockets;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Connections.WebSockets;
    using NUnit.Framework;

    [TestFixture]
    public class WebSocketReceiveResultProxyTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var socketResult = new WebSocketReceiveResult(
                55,
                WebSocketMessageType.Text,
                true,
                WebSocketCloseStatus.InternalServerError,
                closeStatusDescription: "no Description");

            var proxy = new WebSocketReceiveResultProxy(socketResult);

            Assert.AreEqual(55, proxy.Count);
            Assert.AreEqual(ClientMessageType.Text, proxy.MessageType);
            Assert.IsTrue(proxy.EndOfMessage);
            Assert.AreEqual(ClientConnectionCloseStatus.InternalServerError, proxy.CloseStatus.Value);
            Assert.AreEqual("no Description", proxy.CloseStatusDescription);
        }
    }
}