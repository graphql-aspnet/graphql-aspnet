// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;

    /// <summary>
    /// The default listener for raised subscription events. This object routes recieved subscription
    /// events to the various schema instances  within this application domain. This component IS NOT
    /// responsible for publishing new events, only receieving existing ones.
    /// </summary>
    public sealed class DefaultSubscriptionEventRouter : ISubscriptionEventRouter, IDisposable
    {
        private readonly IGraphEventLogger _logger;
        private readonly SubscribedEventRecievers _allReceivers;
        private readonly int _maxReceiverCount;
        private readonly SemaphoreSlim _eventSendSemaphore;
        private readonly ConcurrentHashSet<Task> _allSubscriberTasks;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionEventRouter" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultSubscriptionEventRouter(IGraphEventLogger logger = null)
        {
            _logger = logger;
            _allReceivers = new SubscribedEventRecievers();
            _allSubscriberTasks = new ConcurrentHashSet<Task>();
            _maxReceiverCount = SubscriptionServerSettings.MaxConcurrentReceiverCount;

            if (_maxReceiverCount < 1)
                _maxReceiverCount = 1;

            _eventSendSemaphore = new SemaphoreSlim(_maxReceiverCount);
        }

        /// <inheritdoc />
        public async Task RaisePublishedEvent(SubscriptionEvent eventData, bool waitForDelivery = false)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(eventData, nameof(eventData));

            _logger?.SubscriptionEventReceived(eventData);
            List<ISubscriptionEventReceiver> receivers = null;

            // capture a list of all listeners to this event
            lock (_allReceivers)
            {
                // if no one is listening for the event, just let it go
                var eventName = eventData.ToSubscriptionEventName();
                if (!_allReceivers.ContainsKey(eventName))
                    return;

                receivers = new List<ISubscriptionEventReceiver>(_allReceivers[eventName].Count);
                receivers.AddRange(_allReceivers[eventName]);
            }

            // dispatch the event to the listeners
            var subscriberTasks = new List<Task>();
            if (receivers != null && receivers.Count > 0)
            {
                foreach (var receiver in receivers)
                {
                    var task = this.TransmitEventToReceiver(receiver, eventData);
                    _allSubscriberTasks.Add(task);
                    subscriberTasks.Add(task);
                    _ = task.ContinueWith(this.CleanupEventTransmission);
                }

                if (waitForDelivery)
                    await Task.WhenAll(subscriberTasks).ConfigureAwait(false);
            }
        }

        private async Task TransmitEventToReceiver(ISubscriptionEventReceiver receiver, SubscriptionEvent eventData)
        {
            await _eventSendSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                await receiver.ReceiveEvent(eventData).ConfigureAwait(false);
            }
            finally
            {
                _eventSendSemaphore.Release();
            }
        }

        private Task CleanupEventTransmission(Task eventTask)
        {
            _allSubscriberTasks.TryRemove(eventTask);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void AddReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(eventName, nameof(eventName));
            Validation.ThrowIfNull(receiver, nameof(receiver));

            lock (_allReceivers)
            {
                if (!_allReceivers.ContainsKey(eventName))
                    _allReceivers.Add(eventName, new HashSet<ISubscriptionEventReceiver>());

                _allReceivers[eventName].Add(receiver);
            }
        }

        /// <inheritdoc />
        public void RemoveReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            if (receiver == null || eventName == null)
                return;

            lock (_allReceivers)
            {
                if (_allReceivers.ContainsKey(eventName))
                {
                    if (_allReceivers[eventName].Contains(receiver))
                        _allReceivers[eventName].Remove(receiver);
                    if (_allReceivers[eventName].Count == 0)
                        _allReceivers.Remove(eventName);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveReceiver(ISubscriptionEventReceiver receiver)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            if (receiver == null)
                return;

            lock (_allReceivers)
            {
                var toRemove = new List<SubscriptionEventName>();
                foreach (var kvp in _allReceivers)
                {
                    if (kvp.Value.Contains(receiver))
                        kvp.Value.Remove(receiver);

                    if (kvp.Value.Count == 0)
                        toRemove.Add(kvp.Key);
                }

                // remove any collections that no longer have listeners
                foreach (var key in toRemove)
                    _allReceivers.Remove(key);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                // try and allow any outstanding events some time to release
                if (_allSubscriberTasks.Count > 0)
                {
                    int timeToWaitMs = 1000;
                    while (_allSubscriberTasks.Count > 0 || timeToWaitMs > 0)
                    {
                        Thread.Sleep(500);
                        timeToWaitMs = timeToWaitMs - 500;
                    }
                }

                _eventSendSemaphore.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}