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
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQueryHandlerTests
    {
        [Test]
        public void CallingExecuteYieldsNoExceptionForAGivenBuilder()
        {
            var mock = new Mock<IApplicationBuilder>();
            mock.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                .Verifiable();

            var handler = new TestQueryHandler();
            handler.Execute(mock.Object);

            mock.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once());
        }

        [Test]
        public async Task InvokeWithHttpContext_IsPassedToProcessor()
        {
            var mock = new Mock<IGraphQLHttpProcessor<GraphSchema>>();
            mock.Setup(x => x.Invoke(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var collection = new ServiceCollection();
            collection.AddSingleton(mock.Object);

            var provider = collection.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = provider;

            var handler = new TestQueryHandler();
            await handler.TestInvoke(context);

            mock.Verify(
                x => x.Invoke(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()),
                Times.Once());
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