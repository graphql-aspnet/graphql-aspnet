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
    using System;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Connections.WebSockets;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class WebSocketClientConnectionTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var fakeSocket = new FakeWebSocket(
                WebSocketState.Aborted,
                WebSocketCloseStatus.EndpointUnavailable,
                "close desc",
                "sub proto");

            var context = new DefaultHttpContext();

            var provider = new Mock<IServiceProvider>().Object;
            var securityContext = new Mock<IUserSecurityContext>().Object;

            var user = new ClaimsPrincipal();
            context.RequestServices = provider;
            context.User = user;

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, context);

            Assert.AreEqual(ClientConnectionState.Aborted, client.State);
            Assert.AreEqual(ClientConnectionCloseStatus.EndpointUnavailable, client.CloseStatus.Value);
            Assert.AreEqual("close desc", client.CloseStatusDescription);
            Assert.AreEqual(provider, client.ServiceProvider);
            Assert.AreEqual(user, client.SecurityContext.DefaultUser);
        }

        [Test]
        public async Task Receive_ThrowsSocketException_ReturnsSocketFailureResult()
        {
            var fakeSocket = new FakeWebSocket();

            var exceptionThrown = new WebSocketException("total failure");
            fakeSocket.ThrowExceptionOnReceieve(exceptionThrown);

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as WebSocketFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task Receive_ThrowsGeneralException_ReturnsConnectionFailureResult()
        {
            var fakeSocket = new FakeWebSocket();

            var exceptionThrown = new Exception("total failure");
            fakeSocket.ThrowExceptionOnReceieve(exceptionThrown);

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as ClientConnectionFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task Receive_ThrowsAggregateException_UnwindsAggregateException_ToClientFailureResult()
        {
            var fakeSocket = new FakeWebSocket();

            var exceptionThrown = new Exception("total failure");
            var aggregate = new AggregateException(exceptionThrown);
            fakeSocket.ThrowExceptionOnReceieve(aggregate);

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as ClientConnectionFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task Receive_ThrowsAggregateException_UnwindsAggregateException_ToWebSocketFailureResult()
        {
            var fakeSocket = new FakeWebSocket();

            var exceptionThrown = new WebSocketException("total failure");
            var aggregate = new AggregateException(exceptionThrown);
            fakeSocket.ThrowExceptionOnReceieve(aggregate);

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            var result = await client.ReceiveAsync(array, default);

            var failureResult = result as WebSocketFailureResult;
            Assert.IsNotNull(failureResult);
            Assert.AreEqual(exceptionThrown, failureResult.Exception);
        }

        [Test]
        public async Task SuccessfulRecieve_HasRecievedFromSocket()
        {
            var fakeSocket = new FakeWebSocket();

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            var result = await client.ReceiveAsync(array, default);

            Assert.AreEqual(1, fakeSocket.TotalCallsToReceive);
        }

        [Test]
        public async Task SuccessfulSend_HasSentToSocket()
        {
            var fakeSocket = new FakeWebSocket();

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            await client.SendAsync(array, ClientMessageType.Binary, true, default);

            Assert.AreEqual(1, fakeSocket.TotalCallsToSend);
        }

        [Test]
        public async Task SuccessfulCLose_HasSentToSocket()
        {
            var fakeSocket = new FakeWebSocket();

            var array = new ArraySegment<byte>(new byte[500]);
            var client = new WebSocketClientConnection(fakeSocket, new DefaultHttpContext());
            await client.CloseAsync(ClientConnectionCloseStatus.Empty, string.Empty, default);

            Assert.AreEqual(1, fakeSocket.TotalCloseCalls);
        }
    }
}