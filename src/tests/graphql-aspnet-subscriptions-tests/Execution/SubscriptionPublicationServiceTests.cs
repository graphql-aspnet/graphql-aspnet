// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Execution
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionPublicationServiceTests
    {
        [Test]
        public void StaticWaitInterval_AcceptsNewValues()
        {
            var startvalue = SubscriptionPublicationService.WaitIntervalInMilliseconds;

            SubscriptionPublicationService.WaitIntervalInMilliseconds = startvalue + 1;

            var result = SubscriptionPublicationService.WaitIntervalInMilliseconds;

            Assert.AreEqual(result, startvalue + 1);
            SubscriptionPublicationService.WaitIntervalInMilliseconds = startvalue;
        }

        [Test]
        public async Task PollEventQueue_EmptiesQueueOfEventsAndPublishesThem()
        {
            var logger = new Mock<IGraphEventLogger>();
            logger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<SubscriptionEventPublishedLogEntry>>()));

            var publisher = new Mock<ISubscriptionEventPublisher>();
            publisher.Setup(x => x.PublishEvent(It.IsAny<SubscriptionEvent>())).Returns(Task.CompletedTask);

            var collection = new ServiceCollection();
            collection.AddSingleton<IGraphEventLogger>(logger.Object);
            collection.AddSingleton<ISubscriptionEventPublisher>(publisher.Object);
            var provider = collection.BuildServiceProvider();

            var eventData = new SubscriptionEvent()
            {
                Id = Guid.NewGuid().ToString(),
                SchemaTypeName = "someSchemaName",
                Data = new object(),
                DataTypeName = "someTypeName",
                EventName = "testEvent",
            };

            var eventQueue = new SubscriptionEventQueue();
            eventQueue.Enqueue(eventData);

            var publicationService = new SubscriptionPublicationService(provider, eventQueue);
            await publicationService.PollEventQueue();

            logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<Func<IGraphLogEntry>>()), Times.Once(), "logger not called exactly once");
            publisher.Verify(x => x.PublishEvent(It.IsAny<SubscriptionEvent>()), Times.Once(), "published failed to publish one times");
            Assert.AreEqual(0, eventQueue.Count);
        }
    }
}