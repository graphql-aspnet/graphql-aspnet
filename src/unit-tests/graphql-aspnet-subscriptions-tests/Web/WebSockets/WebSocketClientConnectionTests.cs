// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Web.WebSockets
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.WebSockets;
    using GraphQL.Subscriptions.Tests.Mocks;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class WebSocketClientConnectionTests
    {
        [Test]
        public async Task GeneralPropertyCheck()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER, "sub proto");

            var provider = new Mock<IServiceProvider>().Object;
            var securityContext = new Mock<IUserSecurityContext>().Object;

            var user = new ClaimsPrincipal();
            context.RequestServices = provider;
            context.User = user;

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Aborted,
                        WebSocketCloseStatus.EndpointUnavailable,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(context, fakeSocketManager.Object);
            await client.OpenAsync("actual protocol");

            Assert.AreEqual(ClientConnectionState.Aborted, client.State);
            Assert.AreEqual(ConnectionCloseStatus.EndpointUnavailable, client.CloseStatus.Value);
            Assert.AreEqual("close desc", client.CloseStatusDescription);
            Assert.AreEqual(provider, client.ServiceProvider);
            Assert.AreEqual(user, client.SecurityContext.DefaultUser);
            Assert.AreEqual("sub proto", client.RequestedProtocols);
            Assert.AreEqual("actual protocol", client.Protocol);
        }

        [Test]
        public void SendOnUnopenConnection_ThrowsGeneralException()
        {
            var exceptionThrown = new Exception("total failure");
            var fakeSocketManager = new Mock<WebSocketManager>();

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);
            var data = new byte[] { 1, 2, 3 };
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await client.SendAsync(data, ClientMessageType.Text, true);
            });
        }

        [Test]
        public async Task CloseANonOpenedConnection_MarksAsCLosed()
        {
            var exceptionThrown = new WebSocketException("total failure");
            var fakeSocketManager = new Mock<WebSocketManager>();
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.CloseAsync(ConnectionCloseStatus.Unknown, string.Empty);

            Assert.IsTrue(client.ClosedForever);
        }

        [Test]
        public async Task WebSocketExceptionThrownOnClose_IgnoredForPrematureClosure()
        {
            var exceptionToThrow = new WebSocketException(WebSocketError.ConnectionClosedPrematurely);
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                 .Returns((string protocolName) =>
                 {
                     var socket = new FakeWebSocket(
                         WebSocketState.Open,
                         closeDescription: "close desc",
                         subProtocol: protocolName);

                     socket.ThrowExceptionOnClose(exceptionToThrow);
                     return Task.FromResult(socket as WebSocket);
                 });

            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("some protocol");
            await client.CloseAsync(ConnectionCloseStatus.Unknown, string.Empty);

            Assert.IsTrue(client.ClosedForever);
        }

        [Test]
        public async Task UnanticipatedWebSocketExceptionOnClose_IsReThrown()
        {
            var exceptionToThrow = new WebSocketException(WebSocketError.InvalidState);
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                 .Returns((string protocolName) =>
                 {
                     var socket = new FakeWebSocket(
                         WebSocketState.Open,
                         closeDescription: "close desc",
                         subProtocol: protocolName);

                     socket.ThrowExceptionOnClose(exceptionToThrow);
                     return Task.FromResult(socket as WebSocket);
                 });

            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("some protocol");

            var thrownException = Assert.ThrowsAsync<WebSocketException>(async () =>
            {
                await client.CloseAsync(ConnectionCloseStatus.Unknown, string.Empty);
            });

            Assert.AreSame(exceptionToThrow, thrownException);
            Assert.IsTrue(client.ClosedForever);
        }

        [Test]
        public async Task ExceptionThrownOnClose_IsRethrown()
        {
            var exceptionToThrow = new Exception("total failure");
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                 .Returns((string protocolName) =>
                 {
                     var socket = new FakeWebSocket(
                         WebSocketState.Open,
                         closeDescription: "close desc",
                         subProtocol: protocolName);

                     socket.ThrowExceptionOnClose(exceptionToThrow);
                     return Task.FromResult(socket as WebSocket);
                 });

            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("some protocol");

            var thrownException = Assert.ThrowsAsync<Exception>(async () =>
            {
                await client.CloseAsync(ConnectionCloseStatus.Unknown, string.Empty);
            });

            Assert.AreSame(exceptionToThrow, thrownException);
            Assert.IsTrue(client.ClosedForever);
        }

        [Test]
        public async Task OpenAnAlreadyOpenRequest_ThrowsException()
        {
            var exceptionThrown = new WebSocketException("total failure");

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await client.OpenAsync("protocol");
            });
        }

        [Test]
        public void Receive_OnUnOpenedConnection_ThrowsException()
        {
            var exceptionThrown = new WebSocketException("total failure");

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    socket.ThrowExceptionOnReceieve(exceptionThrown);
                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var result = await client.ReceiveAsync(array, default);
            });
        }

        [Test]
        public async Task Receive_ThrowsSocketException_ReturnsSocketFailureResult()
        {
            var exceptionThrown = new WebSocketException("total failure");

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    socket.ThrowExceptionOnReceieve(exceptionThrown);
                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as WebSocketFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task Receive_ThrowsGeneralException_ReturnsConnectionFailureResult()
        {
            var exceptionThrown = new Exception("total failure");

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    socket.ThrowExceptionOnReceieve(exceptionThrown);
                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as ClientConnectionFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task Receive_ThrowsAggregateException_UnwindsAggregateException_ToClientFailureResult()
        {
            var exceptionThrown = new Exception("total failure");
            var aggregate = new AggregateException(exceptionThrown);

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    socket.ThrowExceptionOnReceieve(aggregate);
                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as ClientConnectionFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task Receive_ThrowsAggregateException_UnwindsAggregateException_ToWebSocketFailureResult()
        {
            var exceptionThrown = new WebSocketException("total failure");
            var aggregate = new AggregateException(exceptionThrown);

            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    var socket = new FakeWebSocket(
                        WebSocketState.Aborted,
                        WebSocketCloseStatus.EndpointUnavailable,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    socket.ThrowExceptionOnReceieve(aggregate);
                    return Task.FromResult(socket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as WebSocketFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task SuccessfulRecieve_HasRecievedFromSocket()
        {
            FakeWebSocket fakeSocket = null;
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    fakeSocket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    return Task.FromResult(fakeSocket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            var result = await client.ReceiveAsync(array, default);

            Assert.IsNotNull(fakeSocket);
            Assert.AreEqual(1, fakeSocket.TotalCallsToReceive);
        }

        [Test]
        public async Task SuccessfulRecieveFullMessage_HasRecievedFromSocket()
        {
            FakeWebSocket fakeSocket = null;
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    fakeSocket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    return Task.FromResult(fakeSocket as WebSocket);
                });

            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            var stream = new MemoryStream();
            var result = await client.ReceiveFullMessage(stream);

            Assert.IsNotNull(fakeSocket);
            Assert.AreEqual(1, fakeSocket.TotalCallsToReceive);
            Assert.IsFalse(result.CloseStatus.HasValue);
            Assert.IsTrue(stream.Length > 0);
        }

        [Test]
        public async Task SuccessfulSend_HasSentToSocket()
        {
            FakeWebSocket fakeSocket = null;
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    fakeSocket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    return Task.FromResult(fakeSocket as WebSocket);
                });

            var array = new byte[500];
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            await client.SendAsync(array, ClientMessageType.Binary, true, default);

            Assert.AreEqual(1, fakeSocket.TotalCallsToSend);
        }

        [Test]
        public async Task SuccessfulCLose_HasSentToSocket()
        {
            FakeWebSocket fakeSocket = null;
            var fakeSocketManager = new Mock<WebSocketManager>();
            fakeSocketManager.Setup(x => x.AcceptWebSocketAsync(It.IsAny<string>()))
                .Returns((string protocolName) =>
                {
                    fakeSocket = new FakeWebSocket(
                        WebSocketState.Open,
                        closeDescription: "close desc",
                        subProtocol: protocolName);

                    return Task.FromResult(fakeSocket as WebSocket);
                });

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(new DefaultHttpContext(), fakeSocketManager.Object);

            await client.OpenAsync("protocol");
            await client.CloseAsync(ConnectionCloseStatus.Empty, string.Empty, default);

            Assert.AreEqual(1, fakeSocket.TotalCloseCalls);
        }
    }
}