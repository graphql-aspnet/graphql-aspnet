// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Defaults
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class HttpSubscriptionMiddlewareTests
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
            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                server.Object,
                options);

            var context = new DefaultHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            // not a socket request aginst the route
            // should be skipped
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
            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                server.Object,
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
            connection.Setup(x => x.StartConnection()).Returns(Task.CompletedTask);

            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.RegisterNewClient(It.IsAny<IClientConnection>())).ReturnsAsync(connection.Object);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                server.Object,
                options);

            var context = new FakeWebSocketHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            // not a socket request aginst the route
            // next middleware component should not be invoked
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
            var server = new Mock<ISubscriptionServer<GraphSchema>>();
            server.Setup(x => x.RegisterNewClient(It.IsAny<IClientConnection>())).Throws(new InvalidOperationException("failed"));

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                server.Object,
                options);

            var context = new FakeWebSocketHttpContext();
            context.RequestServices = new Mock<IServiceProvider>().Object;
            context.Request.Path = options.Route;

            // not a socket request aginst the route
            // next middleware component should not be invoked
            await middleware.InvokeAsync(context);
            Assert.IsFalse(nextCalled);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        }
    }
}