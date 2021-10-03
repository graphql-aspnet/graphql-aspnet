// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Middleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class QueryPipelineComponentTests
    {
        public Task EmptyNextDelegate(GraphQueryExecutionContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        [Test]
        public void ValidateRequestMiddleware_NullContext_ThrowsException()
        {
            var component = new ValidateQueryRequestMiddleware();

            Assert.ThrowsAsync<GraphExecutionException>(() =>
            {
                return component.InvokeAsync(null, EmptyNextDelegate, default);
            });
        }

        [Test]
        public async Task ValidateRequestMiddleware_EmptyQueryText_YieldsCriticalMessage()
        {
            var component = new ValidateQueryRequestMiddleware();
            var req = new Mock<IGraphOperationRequest>();
            req.Setup(x => x.QueryText).Returns(null as string);
            var context = new GraphQueryExecutionContext(
                req.Object,
                new Mock<IServiceProvider>().Object);

            await component.InvokeAsync(context, EmptyNextDelegate, default);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages[0].Severity);
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, context.Messages[0].Code);
        }
    }
}