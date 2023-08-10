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
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionPublicationServiceTests
    {
        [Test]
        public async Task PollEventQueue_EmptiesQueueOfEventsAndPublishesThem()
        {
            var logger = Substitute.For<IGraphEventLogger>();

            var publisher = Substitute.For<ISubscriptionEventPublisher>();

            var collection = new ServiceCollection();
            collection.AddSingleton(logger);
            collection.AddSingleton(publisher);
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

            logger.Received(1).Log(LogLevel.Debug, Arg.Any<Func<IGraphLogEntry>>());
            await publisher.Received(1).PublishEventAsync(Arg.Any<SubscriptionEvent>(), Arg.Any<CancellationToken>());
        }
    }
}