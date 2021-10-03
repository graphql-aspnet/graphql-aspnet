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
    public class WebSocketFailureResultTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var socketException = new WebSocketException(WebSocketError.Faulted, "Failure occured");
            var socketResult = new WebSocketFailureResult(socketException);

            Assert.AreEqual(socketException, socketResult.Exception);
            Assert.AreEqual(socketException.Message, socketResult.CloseStatusDescription);
            Assert.AreEqual(0, socketResult.Count);
            Assert.AreEqual(true, socketResult.EndOfMessage);
            Assert.AreEqual(ClientMessageType.Close, socketResult.MessageType);
            Assert.AreEqual(ClientConnectionCloseStatus.InternalServerError, socketResult.CloseStatus);
        }
    }
}