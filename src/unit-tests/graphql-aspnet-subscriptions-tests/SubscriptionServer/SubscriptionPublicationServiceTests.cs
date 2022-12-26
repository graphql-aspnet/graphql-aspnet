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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.BackgroundServices;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionPublicationServiceTests
    {
        [Test]
        public async Task PollEventQueue_EmptiesQueueOfEventsAndPublishesThem()
        {
            var logger = new Mock<IGraphEventLogger>();
            logger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<SubscriptionEventPublishedLogEntry>>()));

            var publisher = new Mock<ISubscriptionEventPublisher>();
            publisher.Setup(x => x.PublishEventAsync(It.IsAny<SubscriptionEvent>(), It.IsAny<CancellationToken>()));

            var collection = new ServiceCollection();
            collection.AddSingleton(logger.Object);
            collection.AddSingleton(publisher.Object);
            var provider = collection.BuildServiceProvider();

            var eventData = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                SchemaTypeName = "someSchemaName",
                Data = new object(),
                DataTypeName = "someTypeName",
                EventName = "testEvent",
            };

            var eventQueue = new SubscriptionEventPublishingQueue();
            eventQueue.Enqueue(eventData);

            var publicationService = new SubscriptionPublicationService(provider, eventQueue);

            var source = new CancellationTokenSource(50);
            try
            {
                await publicationService.PollQueueAsync(source.Token);
            }
            catch (OperationCanceledException)
            {
            }

            logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<Func<IGraphLogEntry>>()), Times.Once(), "logger not called exactly once");
            publisher.Verify(x => x.PublishEventAsync(It.IsAny<SubscriptionEvent>(), It.IsAny<CancellationToken>()), Times.Once(), "published failed to publish one times");
        }
    }
}