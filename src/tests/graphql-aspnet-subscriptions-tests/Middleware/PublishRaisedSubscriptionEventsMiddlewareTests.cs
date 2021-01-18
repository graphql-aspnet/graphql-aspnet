// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Middleware
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class PublishRaisedSubscriptionEventsMiddlewareTests
    {
        [Test]
        public async Task NoItemsOnContext_YieldsNothingPublished()
        {
            var nextCalled = false;
            Task CallNext(GraphQueryExecutionContext context, CancellationToken token)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            await publisher.InvokeAsync(context, next, default);
            Assert.IsTrue(nextCalled);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public async Task EmptyCollectionOnContext_YieldsNothingPublished()
        {
            var nextCalled = false;
            Task CallNext(GraphQueryExecutionContext context, CancellationToken token)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            var col = context.Items.GetOrAdd(
                SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, (_) => new List<SubscriptionEventProxy>());

            await publisher.InvokeAsync(context, next, default);
            Assert.IsTrue(nextCalled);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void CollectionKeyIsNotACollection_ThrowsException()
        {
            Task CallNext(GraphQueryExecutionContext context, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            var col = context.Items.GetOrAdd(
                SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, (_) => new object());

            Assert.ThrowsAsync<GraphExecutionException>(async () =>
            {
                await publisher.InvokeAsync(context, next, default);
            });
        }

        [Test]
        public async Task QueuedEventProxy_IsPublishedToEventQueue()
        {
            var nextCalled = false;
            Task CallNext(GraphQueryExecutionContext context, CancellationToken token)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            var col = context.Items.GetOrAdd(
                SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, (_) => new List<SubscriptionEventProxy>())
                as List<SubscriptionEventProxy>;

            col.Add(new SubscriptionEventProxy("fakeEvent", new TwoPropertyObject()));

            await publisher.InvokeAsync(context, next, default);
            Assert.IsTrue(nextCalled);
            Assert.AreEqual(1, queue.Count);
        }
    }
}