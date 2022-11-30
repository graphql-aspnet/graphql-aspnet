// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionClientDispatchQueueTests
    {
        [Test]
        public async Task QueuedEvents_AreDispatchedWhenQueueIsProcessed()
        {
            var receiver = new Mock<ISubscriptionClientProxy>();
            receiver.Setup(x => x.Id).Returns(SubscriptionClientId.NewClientId());

            var collection = new Mock<IGlobalSubscriptionClientProxyCollection>();
            collection.Setup(x => x.TryGetClient(It.IsAny<SubscriptionClientId>(), out It.Ref<ISubscriptionClientProxy>.IsAny))
                .Returns((SubscriptionClientId id, out ISubscriptionClientProxy proxy) =>
                {
                    proxy = receiver.Object;
                    return true;
                });

            var evt = new SubscriptionEvent();
            var evt2 = new SubscriptionEvent();

            var queue = new SubscriptionClientDispatchQueue(
                collection.Object,
                maxConcurrentEvents: 1);
            var wasQueued = queue.EnqueueEvent(receiver.Object.Id, evt, true);

            await queue.BeginProcessingQueue();

            Assert.True(wasQueued);
            receiver.Verify(x => x.ReceiveEvent(evt, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task QueuedEvents_AgainstANotFOundCLient_AreNotDispatched()
        {
            var collection = new Mock<IGlobalSubscriptionClientProxyCollection>();

            var evt = new SubscriptionEvent();
            var evt2 = new SubscriptionEvent();

            var queue = new SubscriptionClientDispatchQueue(
                collection.Object,
                maxConcurrentEvents: 1);
            var wasQueued = queue.EnqueueEvent(SubscriptionClientId.NewClientId(), evt, true);

            await queue.BeginProcessingQueue();

            Assert.True(wasQueued);
            Assert.AreEqual(0, queue.Count);
        }

        [TestCase(0)]
        [TestCase(-45)]
        public void CreatingWithMaxConcurrentLessThan1_CreatesWith1(int maxConcurrent)
        {
            var collection = new Mock<IGlobalSubscriptionClientProxyCollection>();

            var queue = new SubscriptionClientDispatchQueue(
                collection.Object,
                maxConcurrentEvents: maxConcurrent);
            Assert.AreEqual(1, queue.MaxConcurrentEvents);
        }

        [Test]
        public void StartingAStoppedQueue_ThrowsException()
        {
            var collection = new Mock<IGlobalSubscriptionClientProxyCollection>();

            var queue = new SubscriptionClientDispatchQueue(
                collection.Object,
                maxConcurrentEvents: 1);

            queue.StopQueue();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await queue.BeginProcessingQueue();
            });
        }

        [Test]
        public void StartingADisposedQueue_ThrowsException()
        {
            var collection = new Mock<IGlobalSubscriptionClientProxyCollection>();
            var queue = new SubscriptionClientDispatchQueue(
                collection.Object,
                maxConcurrentEvents: 1);
            queue.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await queue.BeginProcessingQueue();
            });
        }

        [Test]
        public void StoppingADisposedQueue_DoesNothing()
        {
            var collection = new Mock<IGlobalSubscriptionClientProxyCollection>();
            var queue = new SubscriptionClientDispatchQueue(
                collection.Object,
                maxConcurrentEvents: 1);
            queue.Dispose();
            queue.StopQueue();
        }
    }
}
