// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Subscriptions.Exceptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.Subscriptions.Tests.Mock;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultGraphQLHttpSubscriptionMiddlewareTests
    {
        public class FakeWebSocketManager : WebSocketManager
        {
            public override bool IsWebSocketRequest => true;

            public override IList<string> WebSocketRequestedProtocols => new List<string>();

            public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
            {
                return Task.FromResult(new FakeWebSocket(WebSocketState.Open) as WebSocket);
            }
        }

        public class FakeWebSocketHttpContext : DefaultHttpContext
        {
            public override WebSocketManager WebSockets => new FakeWebSocketManager();
        }

        [Test]
        public async Task NonSocketRequest_AgainstListeningRoute_IsSkipped()
        {
            bool nextCalled = false;
            Task CallNext(HttpContext context)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var factory = new Mock<ISubscriptionServerClientFactory>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new DefaultHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            await middleware.InvokeAsync(context);
            Assert.IsTrue(nextCalled);
        }

        [Test]
        public async Task NonSocketRequest_AgainstDifferentRoute_IsSkipped()
        {
            bool nextCalled = false;
            Task CallNext(HttpContext context)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var factory = new Mock<ISubscriptionServerClientFactory>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new DefaultHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = "/stuff/data";

            // not a socket request aginst the route
            // should be skipped
            await middleware.InvokeAsync(context);
            Assert.IsTrue(nextCalled);
        }

        [Test]
        public async Task SocketRequest_AgainstListeningRoute_SocketIsInvoked()
        {
            bool nextCalled = false;
            Task CallNext(HttpContext context)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);

            var connection = new Mock<ISubscriptionClientProxy>();
            connection.Setup(x => x.StartConnectionAsync(It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var factory = new Mock<ISubscriptionServerClientFactory>();
            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();

            factory.Setup(x => x.CreateSubscriptionClient<GraphSchema>(It.IsAny<IClientConnection>()))
                .ReturnsAsync(client.Object);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            await middleware.InvokeAsync(context);
            Assert.IsFalse(nextCalled);
        }

        [Test]
        public async Task SocketRequest_AgainstListeningRoute_ExceptionOnClientCreation_ResultsInBadRequest()
        {
            bool nextCalled = false;
            Task CallNext(HttpContext context)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);
            var factory = new Mock<ISubscriptionServerClientFactory>();
            factory.Setup(x => x.CreateSubscriptionClient<GraphSchema>(It.IsAny<IClientConnection>()))
                .Throws(new InvalidOperationException("failed"));

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            await middleware.InvokeAsync(context);
            Assert.IsFalse(nextCalled);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        }

        [Test]
        public async Task UnsupportedProtcol_Yields400Error()
        {
            Task CallNext(HttpContext context)
            {
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var factory = new Mock<ISubscriptionServerClientFactory>();

            factory.Setup(x => x.CreateSubscriptionClient<GraphSchema>(It.IsAny<IClientConnection>()))
                .Throws(new UnsupportedClientProtocolException("failed protocol"));

            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.Request.Host = new HostString("localhost:3000");
            context.Request.Headers[SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER] = "unknown-protocl";
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            // not a socket request aginst the route
            // should be skipped
            await middleware.InvokeAsync(context);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        }

        [Test]
        public async Task UnauthenticatedConnection_WhenAuthRequired_ClosesConnection()
        {
            Task CallNext(HttpContext context)
            {
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);

            var options = new SubscriptionServerOptions<GraphSchema>();
            options.AuthenticatedRequestsOnly = true;

            var factory = new Mock<ISubscriptionServerClientFactory>();

            factory.Setup(x => x.CreateSubscriptionClient<GraphSchema>(It.IsAny<IClientConnection>()))
                .Throws(new Exception("this should not be invoked"));

            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.Request.Host = new HostString("localhost:3000");
            context.Request.Headers[SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER] = "graphql-ws";
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            await middleware.InvokeAsync(context);

            Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [Test]
        public async Task MaxConnectionsReached_NewConnectionIsRejected()
        {
            bool nextCalled = false;
            Task CallNext(HttpContext context)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new RequestDelegate(CallNext);

            var connection = new Mock<ISubscriptionClientProxy>();
            connection.Setup(x => x.StartConnectionAsync(It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var factory = new Mock<ISubscriptionServerClientFactory>();
            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();

            factory.Setup(x => x.CreateSubscriptionClient<GraphSchema>(It.IsAny<IClientConnection>()))
                .ReturnsAsync(client.Object);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory.Object,
                new DefaultGlobalSubscriptionClientProxyCollection(0),
                options);

            var context1 = new FakeWebSocketHttpContext();
            context1.RequestServices = new Mock<IServiceProvider>().Object;
            context1.Request.Path = options.Route;

            await middleware.InvokeAsync(context1);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, context1.Response.StatusCode);
            Assert.IsFalse(nextCalled);
        }
    }
}