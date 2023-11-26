// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionClientDispatchQueueTests
    {
        [Test]
        public async Task QueuedEvents_AreDispatchedWhenQueueIsProcessed()
        {
            var receiver = Substitute.For<ISubscriptionClientProxy>();
            receiver.Id.Returns(SubscriptionClientId.NewClientId());

            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();
            collection.TryGetClient(Arg.Any<SubscriptionClientId>(), out Arg.Any<ISubscriptionClientProxy>())
                .Returns(x =>
                {
                    x[1] = receiver;
                    return true;
                });

            var evt = new SubscriptionEvent();
            var evt2 = new SubscriptionEvent();

            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: 1);
            var wasQueued = queue.EnqueueEvent(receiver.Id, evt, true);

            await queue.BeginProcessingQueueAsync();

            Assert.True(wasQueued);
            await receiver.Received(1).ReceiveEventAsync(evt, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task QueuedEvents_AgainstANotFOundCLient_AreNotDispatched()
        {
            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();

            var evt = new SubscriptionEvent();
            var evt2 = new SubscriptionEvent();

            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: 1);
            var wasQueued = queue.EnqueueEvent(SubscriptionClientId.NewClientId(), evt, true);

            await queue.BeginProcessingQueueAsync();

            Assert.True(wasQueued);
            Assert.AreEqual(0, queue.Count);
        }

        [TestCase(0)]
        [TestCase(-45)]
        public void CreatingWithMaxConcurrentLessThan1_CreatesWith1(int maxConcurrent)
        {
            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();

            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: maxConcurrent);
            Assert.AreEqual(1, queue.MaxConcurrentEvents);
        }

        [Test]
        public void StartingAStoppedQueue_ThrowsException()
        {
            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();

            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: 1);

            queue.StopQueue();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await queue.BeginProcessingQueueAsync();
            });
        }

        [Test]
        public void StartingADisposedQueue_ThrowsException()
        {
            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();
            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: 1);
            queue.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await queue.BeginProcessingQueueAsync();
            });
        }

        [Test]
        public void StoppingADisposedQueue_DoesNothing()
        {
            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();
            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: 1);
            queue.Dispose();
            queue.StopQueue();
        }

        [Test]
        public void CountOnDisposedQueue_ThrowsException()
        {
            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();
            var queue = new SubscriptionClientDispatchQueue(
                collection,
                maxConcurrentEvents: 1);
            queue.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                var c = queue.Count;
            });
        }

        [Test]
        public void AlerterAllowedToTest_WhenPresent()
        {
            var factory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();
            factory.CreateLogger(Arg.Any<string>())
                .Returns(logger);

            var collection = Substitute.For<IGlobalSubscriptionClientProxyCollection>();
            var settings = Substitute.For<ISubscriptionClientDispatchQueueAlertSettings>();
            settings.AlertThresholds
                .Returns(new List<SubscriptionEventAlertThreshold>()
                {
                    new SubscriptionEventAlertThreshold(LogLevel.Warning, 45, TimeSpan.FromSeconds(23)),
                });

            var evt = new SubscriptionEvent();

            var queue = new SubscriptionClientDispatchQueue(
                collection,
                settings,
                factory,
                maxConcurrentEvents: 1);

            var wasQueued = queue.EnqueueEvent(SubscriptionClientId.NewClientId(), evt);

            Assert.IsTrue(wasQueued);
            var count = settings.ReceivedCalls()
                .Count(x => x.GetMethodInfo().Name.Contains(nameof(ISubscriptionClientDispatchQueueAlertSettings.AlertThresholds)));
            Assert.IsTrue(count > 0);
        }
    }
}