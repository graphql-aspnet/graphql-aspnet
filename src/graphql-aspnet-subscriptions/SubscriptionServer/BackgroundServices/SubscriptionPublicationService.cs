// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.BackgroundServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// An internal service to which events are queued for publishing. Allows for
    /// "publishing" from a disconnected task so as not to imped the execution of the
    /// query where the subscription events were initially raised.
    /// </summary>
    internal sealed class SubscriptionPublicationService : BackgroundService
    {
        private readonly SubscriptionEventPublishingQueue _eventsToRaise;
        private readonly IGraphEventLogger _logger;
        private readonly ISubscriptionEventPublisher _publisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionPublicationService" /> class.
        /// </summary>
        /// <param name="provider">The service provider this service
        /// can use to instantiate the publisher instances.</param>
        /// <param name="eventQueue">The singular event queue where all "published" events
        /// are initially staged.</param>
        public SubscriptionPublicationService(IServiceProvider provider, SubscriptionEventPublishingQueue eventQueue)
        {
            _eventsToRaise = Validation.ThrowIfNullOrReturn(eventQueue, nameof(eventQueue));
            Validation.ThrowIfNull(provider, nameof(provider));

            var scope = provider.CreateScope();
            _logger = scope.ServiceProvider.GetService<IGraphEventLogger>();
            _publisher = scope.ServiceProvider.GetRequiredService<ISubscriptionEventPublisher>();
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.PollQueueAsync(stoppingToken);
        }

        /// <summary>
        /// An offload method for the primary Execute of this background service.
        /// Used to gain an entry point for unit testing.
        /// </summary>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>ValueTask.</returns>
        internal async ValueTask PollQueueAsync(CancellationToken cancelToken = default)
        {
            try
            {
                while (await _eventsToRaise.WaitToDequeueAsync(cancelToken))
                {
                    if (_eventsToRaise.TryDequeue(out var raisedEvent))
                    {
                        try
                        {
                            await _publisher.PublishEventAsync(raisedEvent, cancelToken);
                            _logger?.SubscriptionEventPublished(raisedEvent);
                        }
                        catch (Exception ex)
                        {
                            _logger?.UnhandledExceptionEvent(ex);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}