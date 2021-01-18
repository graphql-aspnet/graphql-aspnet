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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// An internal hosted service to which events are queued for publishing. Allows for
    /// sending from a disconnected task so as not to imped the execution of the task/pipeline where
    /// the subscription events were initially raised.
    /// </summary>
    public sealed class SubscriptionPublicationService : BackgroundService
    {
        private static readonly object _syncLock = new object();
        private static int _waitInterval;

        /// <summary>
        /// <para>
        /// Gets or sets the amount of time the internal publication service will
        /// wait when it reaches the end of the event queue before inspecting the queue again.
        /// </para>
        /// <para>
        /// This value should be configured during startup. Once this publication service is started
        /// the value becomes fixed.
        /// </para>
        /// <para>
        /// Default Value: 100ms,  Minimum Value: 15ms.
        /// </para>
        /// </summary>
        /// <value>The amount of time to wait, in milliseconds.</value>
        public static int WaitIntervalInMilliseconds
        {
            get
            {
                lock (_syncLock)
                    return _waitInterval;
            }

            set
            {
                lock (_syncLock)
                    _waitInterval = value;
            }
        }

        /// <summary>
        /// Initializes static members of the <see cref="SubscriptionPublicationService"/> class.
        /// </summary>
        static SubscriptionPublicationService()
        {
            WaitIntervalInMilliseconds = 100;
        }

        private readonly IServiceProvider _provider;
        private readonly SubscriptionEventQueue _eventsToRaise;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionPublicationService" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="eventQueue">The event queue.</param>
        public SubscriptionPublicationService(IServiceProvider provider, SubscriptionEventQueue eventQueue)
        {
            _eventsToRaise = Validation.ThrowIfNullOrReturn(eventQueue, nameof(eventQueue));
            _provider = Validation.ThrowIfNullOrReturn(provider, nameof(provider));
        }

        /// <summary>
        /// This method is called when the <see cref="IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="Task" /> that represents the long running operations.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var waitInterval = SubscriptionPublicationService.WaitIntervalInMilliseconds;
            if (waitInterval < 15)
                waitInterval = 15;

            while (!stoppingToken.IsCancellationRequested)
            {
                await this.PollEventQueue();

                try
                {
                    await Task.Delay(waitInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Forces this publication service to poll its internal queue for new events instead of waiting
        /// for hte next publication cycle.
        /// </summary>
        /// <returns>Task.</returns>
        internal async Task PollEventQueue()
        {
            if (_eventsToRaise.Count > 0)
            {
                using var scope = _provider.CreateScope();
                var logger = scope.ServiceProvider.GetService<IGraphEventLogger>();
                var publisher = scope.ServiceProvider.GetRequiredService<ISubscriptionEventPublisher>();
                while (_eventsToRaise.TryDequeue(out var result))
                {
                    if (result != null)
                    {
                        await publisher.PublishEvent(result);
                        logger?.SubscriptionEventPublished(result);
                    }
                }
            }
        }
    }
}