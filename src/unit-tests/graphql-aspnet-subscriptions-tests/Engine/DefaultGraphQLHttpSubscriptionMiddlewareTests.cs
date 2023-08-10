// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.Exceptions;
    using GraphQL.AspNet.Tests.Engine.TestData;
    using Microsoft.AspNetCore.Http;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultGraphQLHttpSubscriptionMiddlewareTests
    {
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
            var factory = Substitute.For<ISubscriptionServerClientFactory>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new DefaultHttpContext();
            context.RequestServices = Substitute.For<IServiceProvider>();
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
            var factory = Substitute.For<ISubscriptionServerClientFactory>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new DefaultHttpContext();
            context.RequestServices = Substitute.For<IServiceProvider>();
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

            var connection = Substitute.For<ISubscriptionClientProxy>();
            connection.StartConnectionAsync(Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var factory = Substitute.For<ISubscriptionServerClientFactory>();
            var client = Substitute.For<ISubscriptionClientProxy<GraphSchema>>();
            client.Id.Returns(SubscriptionClientId.NewClientId());

            factory.CreateSubscriptionClientAsync<GraphSchema>(Arg.Any<IClientConnection>())
                .Returns(client);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.RequestServices = Substitute.For<IServiceProvider>();
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
            var factory = Substitute.For<ISubscriptionServerClientFactory>();
            factory.CreateSubscriptionClientAsync<GraphSchema>(Arg.Any<IClientConnection>())
                .Throws(new InvalidOperationException("failed"));

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.RequestServices = Substitute.For<IServiceProvider>();
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
            var factory = Substitute.For<ISubscriptionServerClientFactory>();

            factory.CreateSubscriptionClientAsync<GraphSchema>(Arg.Any<IClientConnection>())
                .Throws(new UnsupportedClientProtocolException("failed protocol"));

            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.Request.Host = new HostString("localhost:3000");
            context.Request.Headers[SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER] = "unknown-protocl";
            context.RequestServices = Substitute.For<IServiceProvider>();
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

            var factory = Substitute.For<ISubscriptionServerClientFactory>();

            factory.CreateSubscriptionClientAsync<GraphSchema>(Arg.Any<IClientConnection>())
                .Throws(new Exception("this should not be invoked"));

            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(25),
                options);

            var context = new FakeWebSocketHttpContext();
            context.Request.Host = new HostString("localhost:3000");
            context.Request.Headers[SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER] = "graphql-ws";
            context.RequestServices = Substitute.For<IServiceProvider>();
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

            var connection = Substitute.For<ISubscriptionClientProxy>();
            connection.StartConnectionAsync(
                Arg.Any<TimeSpan?>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var factory = Substitute.For<ISubscriptionServerClientFactory>();
            var client = Substitute.For<ISubscriptionClientProxy<GraphSchema>>();

            factory.CreateSubscriptionClientAsync<GraphSchema>(Arg.Any<IClientConnection>())
                .Returns(client);

            var options = new SubscriptionServerOptions<GraphSchema>();
            var middleware = new DefaultGraphQLHttpSubscriptionMiddleware<GraphSchema>(
                next,
                new GraphSchema(),
                factory,
                new DefaultGlobalSubscriptionClientProxyCollection(0),
                options);

            var context1 = new FakeWebSocketHttpContext();
            context1.RequestServices = Substitute.For<IServiceProvider>();
            context1.Request.Path = options.Route;

            await middleware.InvokeAsync(context1);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, context1.Response.StatusCode);
            Assert.IsFalse(nextCalled);
        }
    }
}