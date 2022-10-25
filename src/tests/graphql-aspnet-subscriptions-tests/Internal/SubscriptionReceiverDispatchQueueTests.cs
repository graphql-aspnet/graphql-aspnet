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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Internal;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionReceiverDispatchQueueTests
    {
        [Test]
        public async Task QueuedEvents_AreDispatchedWithQueueIsProcessed()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();

            var evt = new SubscriptionEvent();
            var evt2 = new SubscriptionEvent();

            var queue = new SubscriptionReceiverDispatchQueue(1);
            var wasQueued = queue.EnqueueEvent(receiver.Object, evt, true);

            await queue.BeginProcessingQueue();

            Assert.True(wasQueued);
            receiver.Verify(x => x.ReceiveEvent(evt, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(0)]
        [TestCase(-45)]
        public void CreatingWithMaxConcurrentLessThan1_CreatesWith1(int maxConcurrent)
        {
            var queue = new SubscriptionReceiverDispatchQueue(maxConcurrent);
            Assert.AreEqual(1, queue.MaxConcurrentEvents);
        }

        [Test]
        public void StartingAStoppedQueue_ThrowsException()
        {
            var queue = new SubscriptionReceiverDispatchQueue(1);

            queue.StopQueue();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await queue.BeginProcessingQueue();
            });
        }

        [Test]
        public void StartingADisposedQueue_ThrowsException()
        {
            var queue = new SubscriptionReceiverDispatchQueue(1);
            queue.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await queue.BeginProcessingQueue();
            });
        }

        [Test]
        public void StoppingADisposedQueue_ThrowsException()
        {
            var queue = new SubscriptionReceiverDispatchQueue(1);
            queue.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                queue.StopQueue();
            });
        }
    }
}
