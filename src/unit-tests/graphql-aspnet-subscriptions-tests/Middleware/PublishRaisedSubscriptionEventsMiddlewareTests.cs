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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class PublishRaisedSubscriptionEventsMiddlewareTests
    {
        [Test]
        public async Task NoItemsOnContext_YieldsNothingPublished()
        {
            var nextCalled = false;
            Task CallNext(QueryExecutionContext context, CancellationToken token)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<QueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventPublishingQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            await publisher.InvokeAsync(context, next, default);
            Assert.IsTrue(nextCalled);
        }

        [Test]
        public async Task EmptyCollectionOnContext_YieldsNothingPublished()
        {
            var nextCalled = false;
            Task CallNext(QueryExecutionContext context, CancellationToken token)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<QueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventPublishingQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            var col = context.Session.Items.GetOrAdd(
                SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION, (_) => new List<SubscriptionEventProxy>());

            await publisher.InvokeAsync(context, next, default);
            Assert.IsTrue(nextCalled);
        }

        [Test]
        public void CollectionKeyIsNotACollection_ThrowsException()
        {
            Task CallNext(QueryExecutionContext context, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<QueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventPublishingQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            var col = context.Session.Items.GetOrAdd(
                SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION, (_) => new object());

            Assert.ThrowsAsync<GraphExecutionException>(async () =>
            {
                await publisher.InvokeAsync(context, next, default);
            });
        }

        [Test]
        public async Task QueuedEventProxy_IsPublishedToEventQueue()
        {
            var nextCalled = false;
            Task CallNext(QueryExecutionContext context, CancellationToken token)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            var next = new GraphMiddlewareInvocationDelegate<QueryExecutionContext>(CallNext);
            var queue = new SubscriptionEventPublishingQueue();
            var publisher = new PublishRaisedSubscriptionEventsMiddleware<GraphSchema>(queue);

            var server = new TestServerBuilder()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .Build();

            var col = context.Session.Items.GetOrAdd(
                SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION, (_) => new List<SubscriptionEventProxy>())
                as List<SubscriptionEventProxy>;

            col.Add(new SubscriptionEventProxy("fakeEvent", new TwoPropertyObject()));

            await publisher.InvokeAsync(context, next, default);
            Assert.IsTrue(nextCalled);
        }
    }
}