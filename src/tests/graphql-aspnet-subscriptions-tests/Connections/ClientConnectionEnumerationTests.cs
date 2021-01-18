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
    using NUnit.Framework;

    [TestFixture]
    public class ClientConnectionEnumerationTests
    {
        [TestCase(WebSocketCloseStatus.Empty, ClientConnectionCloseStatus.Empty)]
        [TestCase(WebSocketCloseStatus.NormalClosure, ClientConnectionCloseStatus.NormalClosure)]
        [TestCase(WebSocketCloseStatus.EndpointUnavailable, ClientConnectionCloseStatus.EndpointUnavailable)]
        [TestCase(WebSocketCloseStatus.ProtocolError, ClientConnectionCloseStatus.ProtocolError)]
        [TestCase(WebSocketCloseStatus.InvalidMessageType, ClientConnectionCloseStatus.InvalidMessageType)]
        [TestCase(WebSocketCloseStatus.InvalidPayloadData, ClientConnectionCloseStatus.InvalidPayloadData)]
        [TestCase(WebSocketCloseStatus.PolicyViolation, ClientConnectionCloseStatus.PolicyViolation)]
        [TestCase(WebSocketCloseStatus.MessageTooBig, ClientConnectionCloseStatus.MessageTooBig)]
        [TestCase(WebSocketCloseStatus.MandatoryExtension, ClientConnectionCloseStatus.MandatoryExtension)]
        [TestCase(WebSocketCloseStatus.InternalServerError, ClientConnectionCloseStatus.InternalServerError)]
        [TestCase(22, 22)]
        public void ToClientConnectionCloseStatus(WebSocketCloseStatus socketStatus, ClientConnectionCloseStatus expectedCloseStatus)
        {
            Assert.AreEqual(expectedCloseStatus, socketStatus.ToClientConnectionCloseStatus());
        }

        [TestCase(ClientConnectionCloseStatus.Empty, WebSocketCloseStatus.Empty)]
        [TestCase(ClientConnectionCloseStatus.NormalClosure, WebSocketCloseStatus.NormalClosure)]
        [TestCase(ClientConnectionCloseStatus.EndpointUnavailable, WebSocketCloseStatus.EndpointUnavailable)]
        [TestCase(ClientConnectionCloseStatus.ProtocolError, WebSocketCloseStatus.ProtocolError)]
        [TestCase(ClientConnectionCloseStatus.InvalidMessageType, WebSocketCloseStatus.InvalidMessageType)]
        [TestCase(ClientConnectionCloseStatus.InvalidPayloadData, WebSocketCloseStatus.InvalidPayloadData)]
        [TestCase(ClientConnectionCloseStatus.PolicyViolation, WebSocketCloseStatus.PolicyViolation)]
        [TestCase(ClientConnectionCloseStatus.MessageTooBig, WebSocketCloseStatus.MessageTooBig)]
        [TestCase(ClientConnectionCloseStatus.MandatoryExtension, WebSocketCloseStatus.MandatoryExtension)]
        [TestCase(ClientConnectionCloseStatus.InternalServerError, WebSocketCloseStatus.InternalServerError)]
        [TestCase(22, 22)]
        public void ToClientConnectionCloseStatus(ClientConnectionCloseStatus socketStatus, WebSocketCloseStatus expectedCloseStatus)
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