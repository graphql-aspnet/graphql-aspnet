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
    using GraphQL.AspNet.Tests.Execution.TimeoutAndCancellationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class TimeoutAndCancellationTests
    {
        [Test]
        public async Task RunTimeTimeout_AResultIsRendered_TimeoutsAreCaptured()
        {
            var instance = new TimeoutCancellationController();

            var serverBuilder = new TestServerBuilder()
                        .AddType<TimeoutCancellationController>();
            serverBuilder.AddSingleton(instance);

            serverBuilder.AddGraphQL(o =>
            {
                o.ExecutionOptions.QueryTimeout = TimeSpan.FromMilliseconds(5);
            });

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {  timedOutMethod(ms: 2000)  }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.OPERATION_TIMEOUT, result.Messages[0].Code);
            Assert.IsTrue(instance.MethodInvoked);
        }

        [Test]
        public async Task ExternalCancellationAfterExecutionStarts_YieldsNoResult()
        {
            var instance = new TimeoutCancellationController();

            var serverBuilder = new TestServerBuilder()
                        .AddType<TimeoutCancellationController>();
            serverBuilder.AddSingleton(instance);

            var server = serverBuilder.Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText("query {  timedOutMethod(ms: 3000)  }")
                .Build();

            // trigger an external cancellation once the controller
            // method is invoked
            var externalCancelSource = new CancellationTokenSource();
            instance.MethodCalled = () =>
            {
                externalCancelSource.Cancel();
            };

            await server.ExecuteQuery(context, externalCancelSource.Token);

            // when truely cancelled there is no result
            Assert.IsNull(context.Result);

            // the context should get a failure message
            // since it was in progress
            Assert.IsTrue(instance.MethodInvoked);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.OPERATION_CANCELED, context.Messages[0].Code);
        }

        [Test]
        public async Task ExternalCancellationBeforeExecutionStarts_YieldsNoResult_AndNoMessages()
        {
            var instance = new TimeoutCancellationController();

            var serverBuilder = new TestServerBuilder()
                        .AddType<TimeoutCancellationController>();
            serverBuilder.AddSingleton(instance);

            var server = serverBuilder.Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText("query {  timedOutMethod(ms: 3000)  }")
                .Build();

            // trigger an external cancellation before the runtime can even
            // be invoked
            var externalCancelSource = new CancellationTokenSource();
            externalCancelSource.Cancel();

            await server.ExecuteQuery(context, externalCancelSource.Token);

            // when truely cancelled there is no result
            Assert.IsNull(context.Result);
            Assert.IsFalse(instance.MethodInvoked); // controller should never be invoked
            Assert.AreEqual(0, context.Messages.Count); // no messages should be recorded
        }
    }
}