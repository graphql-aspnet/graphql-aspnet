// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Web
{
    using System.Net.WebSockets;
    using GraphQL.AspNet.Web;
    using NUnit.Framework;

    [TestFixture]
    public class ClientConnectionEnumerationTests
    {
        [TestCase(WebSocketCloseStatus.Empty, ConnectionCloseStatus.Empty)]
        [TestCase(WebSocketCloseStatus.NormalClosure, ConnectionCloseStatus.NormalClosure)]
        [TestCase(WebSocketCloseStatus.EndpointUnavailable, ConnectionCloseStatus.EndpointUnavailable)]
        [TestCase(WebSocketCloseStatus.ProtocolError, ConnectionCloseStatus.ProtocolError)]
        [TestCase(WebSocketCloseStatus.InvalidMessageType, ConnectionCloseStatus.InvalidMessageType)]
        [TestCase(WebSocketCloseStatus.InvalidPayloadData, ConnectionCloseStatus.InvalidPayloadData)]
        [TestCase(WebSocketCloseStatus.PolicyViolation, ConnectionCloseStatus.PolicyViolation)]
        [TestCase(WebSocketCloseStatus.MessageTooBig, ConnectionCloseStatus.MessageTooBig)]
        [TestCase(WebSocketCloseStatus.MandatoryExtension, ConnectionCloseStatus.MandatoryExtension)]
        [TestCase(WebSocketCloseStatus.InternalServerError, ConnectionCloseStatus.InternalServerError)]
        [TestCase(22, 22)]
        public void ToClientConnectionCloseStatus(WebSocketCloseStatus socketStatus, ConnectionCloseStatus expectedCloseStatus)
        {
            Assert.AreEqual(expectedCloseStatus, socketStatus.ToClientConnectionCloseStatus());
        }

        [TestCase(ConnectionCloseStatus.Empty, WebSocketCloseStatus.Empty)]
        [TestCase(ConnectionCloseStatus.NormalClosure, WebSocketCloseStatus.NormalClosure)]
        [TestCase(ConnectionCloseStatus.EndpointUnavailable, WebSocketCloseStatus.EndpointUnavailable)]
        [TestCase(ConnectionCloseStatus.ProtocolError, WebSocketCloseStatus.ProtocolError)]
        [TestCase(ConnectionCloseStatus.InvalidMessageType, WebSocketCloseStatus.InvalidMessageType)]
        [TestCase(ConnectionCloseStatus.InvalidPayloadData, WebSocketCloseStatus.InvalidPayloadData)]
        [TestCase(ConnectionCloseStatus.PolicyViolation, WebSocketCloseStatus.PolicyViolation)]
        [TestCase(ConnectionCloseStatus.MessageTooBig, WebSocketCloseStatus.MessageTooBig)]
        [TestCase(ConnectionCloseStatus.MandatoryExtension, WebSocketCloseStatus.MandatoryExtension)]
        [TestCase(ConnectionCloseStatus.InternalServerError, WebSocketCloseStatus.InternalServerError)]
        [TestCase(22, 22)]
        public void ToClientConnectionCloseStatus(ConnectionCloseStatus socketStatus, WebSocketCloseStatus expectedCloseStatus)
        {
            Assert.AreEqual(expectedCloseStatus, socketStatus.ToWebSocketCloseStatus());
        }

        [TestCase(WebSocketState.None, ClientConnectionState.None)]
        [TestCase(WebSocketState.Connecting, ClientConnectionState.Connecting)]
        [TestCase(WebSocketState.Open, ClientConnectionState.Open)]
        [TestCase(WebSocketState.CloseSent, ClientConnectionState.CloseSent)]
        [TestCase(WebSocketState.CloseReceived, ClientConnectionState.CloseReceived)]
        [TestCase(WebSocketState.Closed, ClientConnectionState.Closed)]
        [TestCase(WebSocketState.Aborted, ClientConnectionState.Aborted)]
        public void ToClientState(WebSocketState socketState, ClientConnectionState expectedState)
        {
            Assert.AreEqual(expectedState, socketState.ToClientState());
        }

        [TestCase(ClientMessageType.Text, WebSocketMessageType.Text)]
        [TestCase(ClientMessageType.Binary, WebSocketMessageType.Binary)]
        [TestCase(ClientMessageType.Close, WebSocketMessageType.Close)]
        public void ToWebSocketMessageType(ClientMessageType messageType, WebSocketMessageType expectedType)
        {
            Assert.AreEqual(expectedType, messageType.ToWebSocketMessageType());
        }

        [TestCase(WebSocketMessageType.Text, ClientMessageType.Text)]
        [TestCase(WebSocketMessageType.Binary, ClientMessageType.Binary)]
        [TestCase(WebSocketMessageType.Close, ClientMessageType.Close)]
        public void ToClientMessageType(WebSocketMessageType messageType, ClientMessageType expectedType)
        {
            Assert.AreEqual(expectedType, messageType.ToClientMessageType());
        }
    }
}