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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The default listener for raised subscription events. This object routes recieved subscription
    /// events to the various schema instances  within this application domain. This component IS NOT
    /// responsible for publishing new events, only receieving existing ones.
    /// </summary>
    public sealed class DefaultSubscriptionEventRouter : ISubscriptionEventRouter, IDisposable
    {
        private readonly ILogger _logger;
        private readonly SubscribedEventRecievers _allReceivers;
        private readonly ISubscriptionEventDispatchQueue _dispatchQueue;
        private readonly Task _dispatchQueueExecutionTask;
        private bool _isDisposed;

        public DefaultSubscriptionEventRouter(
            ISubscriptionEventDispatchQueue dispatchQueue,
            ILogger logger = null)
        {
            _dispatchQueue = Validation.ThrowIfNullOrReturn(dispatchQueue, nameof(dispatchQueue));
            _logger = logger;
            _allReceivers = new SubscribedEventRecievers();
        }

        /// <inheritdoc />
        public void RaisePublishedEvent(SubscriptionEvent eventData)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(eventData, nameof(eventData));

            _logger?.SubscriptionEventReceived(eventData);
            List<SubscriptionClientId> receivers = null;

            // capture a list of all listeners to this event
            lock (_allReceivers)
            {
                // if no one is listening for the event, just let it go
                var eventName = eventData.ToSubscriptionEventName();
                if (!_allReceivers.ContainsKey(eventName))
                    return;

                receivers = new List<SubscriptionClientId>(_allReceivers[eventName].Count);
                receivers.AddRange(_allReceivers[eventName]);
            }

            // queue all events to be dispatched to the receivers
            if (receivers.Count > 0)
            {
                foreach (var receiver in receivers)
                    _dispatchQueue.EnqueueEvent(receiver, eventData);
            }
        }

        /// <inheritdoc />
        public void AddClient(ISubscriptionClientProxy receiver, SubscriptionEventName eventName)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(eventName, nameof(eventName));
            Validation.ThrowIfNull(receiver, nameof(receiver));

            lock (_allReceivers)
            {
                if (!_allReceivers.ContainsKey(eventName))
                    _allReceivers.Add(eventName, new HashSet<SubscriptionClientId>());

                _allReceivers[eventName].Add(receiver.Id);
            }
        }

        /// <inheritdoc />
        public void RemoveClient(ISubscriptionClientProxy receiver, SubscriptionEventName eventName)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            if (eventName == null)
                return;

            lock (_allReceivers)
            {
                if (_allReceivers.ContainsKey(eventName))
                {
                    if (_allReceivers[eventName].Contains(receiver.Id))
                        _allReceivers[eventName].Remove(receiver.Id);
                    if (_allReceivers[eventName].Count == 0)
                        _allReceivers.Remove(eventName);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveClient(ISubscriptionClientProxy receiver)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(receiver, nameof(receiver));

            lock (_allReceivers)
            {
                var toRemove = new List<SubscriptionEventName>();
                foreach (var kvp in _allReceivers)
                {
                    if (kvp.Value.Contains(receiver.Id))
                        kvp.Value.Remove(receiver.Id);

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

                // try and allow any outstanding events some time to
                // shut down gracefully
                if (_dispatchQueue.IsProcessing)
                    _dispatchQueue.StopQueue();

                int timeToWaitMs = 1000;
                while (_dispatchQueue.IsProcessing && timeToWaitMs > 0)
                {
                    Thread.Sleep(500);
                    timeToWaitMs = timeToWaitMs - 500;
                }

                _dispatchQueue.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}