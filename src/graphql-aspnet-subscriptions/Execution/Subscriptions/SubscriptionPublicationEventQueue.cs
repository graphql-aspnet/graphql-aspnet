// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// An internal hosted service to which events are queued for publishing. Allows for
    /// sending from a disconnected task so as not to imped the execution of the task/pipeline where
    /// the subscription events were initially raised.
    /// </summary>
    public class SubscriptionPublicationEventQueue : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private ConcurrentQueue<SubscriptionEvent> _eventsToRaise;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionPublicationEventQueue"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public SubscriptionPublicationEventQueue(IServiceProvider provider)
        {
            _eventsToRaise = new ConcurrentQueue<SubscriptionEvent>();
            _provider = Validation.ThrowIfNullOrReturn(provider, nameof(provider));
        }

        /// <summary>
        /// Queues the event for publishing on the next heartbeat.
        /// </summary>
        /// <param name="eventData">The event data to queue.</param>
        public void EnqueueEvent(SubscriptionEvent eventData)
        {
            if (eventData != null)
                _eventsToRaise.Enqueue(eventData);
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_eventsToRaise.Count > 0)
                {
                    using var scope = _provider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<ISubscriptionEventPublisher>();
                    while (_eventsToRaise.TryDequeue(out var result))
                    {
                        await publisher.PublishEvent(result);
                    }
                }

                // TODO: make delay deliverable
                await Task.Delay(1000);
            }
        }
    }
}