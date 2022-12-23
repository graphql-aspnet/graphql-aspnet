// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Tests.Execution.TestData.TimeoutAndCancellationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TimeoutAndCancellationTests
    {
        [Test]
        public async Task WhenARuntimeTimeoutOccurs_AResultIsRendered_TimeoutMessageIsCaptured()
        {
            var logger = new Mock<IGraphEventLogger>();

            var instance = new TimeoutCancellationController();

            var serverBuilder = new TestServerBuilder()
                        .AddType<TimeoutCancellationController>();
            serverBuilder.AddSingleton(instance);

            serverBuilder.AddGraphQL(o =>
            {
                o.ExecutionOptions.QueryTimeout = TimeSpan.FromMilliseconds(20);
            });

            var server = serverBuilder.Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText("query {  timedOutMethod(ms: 2000)  }")
                .AddLogger(logger.Object)
                .Build();

            await server.ExecuteQuery(context);
            var result = context;

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.OPERATION_TIMEOUT, result.Messages[0].Code);
            Assert.IsTrue(instance.MethodInvoked); // controller method should be called

            // a timeout should log the timeout operation as well as complete the request
            // with an error message
            logger.Verify(x => x.RequestCancelled(It.IsAny<QueryExecutionContext>()), Times.Never());
            logger.Verify(x => x.RequestTimedOut(It.IsAny<QueryExecutionContext>()), Times.Once());
            logger.Verify(x => x.RequestCompleted(It.IsAny<QueryExecutionContext>()), Times.Once());
        }

        [Test]
        public async Task WhenExternalCancellationOccurs_AfterTheQueryStarts_NoResultIsRendered_ButContextCapturesErrors()
        {
            var logger = new Mock<IGraphEventLogger>();

            var instance = new TimeoutCancellationController();

            var serverBuilder = new TestServerBuilder()
                        .AddType<TimeoutCancellationController>();
            serverBuilder.AddSingleton(instance);

            var server = serverBuilder.Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText("query {  timedOutMethod(ms: 3000)  }")
                .AddLogger(logger.Object)
                .Build();

            // simulated HttpContext supplied CancelToken
            var externalCancelSource = new CancellationTokenSource();

            // trigger an external cancellation once the controller
            // method is invoked
            instance.MethodToInvokeDuringControllerAction = () =>
            {
                externalCancelSource.Cancel();
            };

            await server.ExecuteQuery(context, externalCancelSource.Token);

            // when truely cancelled there is no result/data
            Assert.IsNull(context.Result);

            // the context should get a failure message
            // since it was in progress
            Assert.IsTrue(instance.MethodInvoked);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.OPERATION_CANCELED, context.Messages[0].Code);

            // since the query started, the runtime should process the cancellation
            // but not a request completed
            logger.Verify(x => x.RequestCancelled(It.IsAny<QueryExecutionContext>()), Times.Once());
            logger.Verify(x => x.RequestTimedOut(It.IsAny<QueryExecutionContext>()), Times.Never());
            logger.Verify(x => x.RequestCompleted(It.IsAny<QueryExecutionContext>()), Times.Never());
        }

        [Test]
        public async Task WhenExternalCancellationOccurs_BeforeTheQueryStarts_NoResultIsRendered_ContextCapturesNothing()
        {
            var logger = new Mock<IGraphEventLogger>();
            var instance = new TimeoutCancellationController();

            var serverBuilder = new TestServerBuilder()
                        .AddType<TimeoutCancellationController>();
            serverBuilder.AddSingleton(instance);

            var server = serverBuilder.Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText("query {  timedOutMethod(ms: 3000)  }")
                .AddLogger(logger.Object)
                .Build();

            // trigger an external cancellation before the runtime can even
            // be invoked
            var externalCancelSource = new CancellationTokenSource();
            externalCancelSource.Cancel();

            await server.ExecuteQuery(context, externalCancelSource.Token);

            // Nothing about the runtime was invoked...no messages are captured
            Assert.IsNull(context.Result);
            Assert.IsFalse(instance.MethodInvoked); // controller should never be invoked
            Assert.AreEqual(0, context.Messages.Count); // no messages should be recorded

            // the runtime should never process the message so nothing can be logged.
            logger.Verify(x => x.RequestCancelled(It.IsAny<QueryExecutionContext>()), Times.Never());
            logger.Verify(x => x.RequestTimedOut(It.IsAny<QueryExecutionContext>()), Times.Never());
            logger.Verify(x => x.RequestCompleted(It.IsAny<QueryExecutionContext>()), Times.Never());
        }
    }
}