// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Web.WebTestData;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQueryHandlerTests
    {
        [Test]
        public void CallingExecuteYieldsNoExceptionForAGivenBuilder()
        {
            var mock = Substitute.For<IApplicationBuilder>();
            mock.Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());

            var handler = new TestQueryHandler();
            handler.Execute(mock);

            mock.Received(1).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        }

        [Test]
        public async Task InvokeWithHttpContext_IsPassedToProcessor()
        {
            var mock = Substitute.For<IGraphQLHttpProcessor<GraphSchema>>();

            var collection = new ServiceCollection();
            collection.AddSingleton(mock);

            var provider = collection.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = provider;

            var handler = new TestQueryHandler();
            await handler.TestInvoke(context);

            await mock.Received(1).InvokeAsync(Arg.Any<HttpContext>());
        }

        [Test]
        public void InvokeWithMissingHttpContext_ExceptionisThrown()
        {
            var handler = new TestQueryHandler();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await handler.TestInvoke(null);
            });
        }

        [Test]
        public void InvokeWithMissingServiceProvider_ExceptionisThrown()
        {
            var handler = new TestQueryHandler();

            var context = new DefaultHttpContext();
            context.RequestServices = null;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await handler.TestInvoke(null);
            });
        }

        [Test]
        public void InvokeWithMissingProcessor_ExceptionIsTHrown()
        {
            var collection = new ServiceCollection();
            var provider = collection.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = provider;

            var handler = new TestQueryHandler();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await handler.TestInvoke(context);
            });
        }
    }
}