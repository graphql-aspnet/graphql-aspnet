// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Internal.Interfaces;
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
        private readonly SubscribedEventRecievers _allclients;
        private readonly ISubscriptionEventDispatchQueue _dispatchQueue;
        private readonly Task _dispatchQueueExecutionTask;
        private bool _isDisposed;

        public DefaultSubscriptionEventRouter(
            ISubscriptionEventDispatchQueue dispatchQueue,
            ILogger logger = null)
        {
            _dispatchQueue = Validation.ThrowIfNullOrReturn(dispatchQueue, nameof(dispatchQueue));
            _logger = logger;
            _allclients = new SubscribedEventRecievers();
        }

        /// <inheritdoc />
        public void RaisePublishedEvent(SubscriptionEvent eventData)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(eventData, nameof(eventData));

            _logger?.SubscriptionEventReceived(eventData);
            List<SubscriptionClientId> clients = null;

            // capture a list of all listeners to this event
            lock (_allclients)
            {
                // if no one is listening for the event, just let it go
                var eventName = eventData.ToSubscriptionEventName();
                if (!_allclients.ContainsKey(eventName))
                    return;

                clients = new List<SubscriptionClientId>(_allclients[eventName].Count);
                clients.AddRange(_allclients[eventName]);
            }

            // queue all events to be dispatched to the clients
            if (clients.Count > 0)
            {
                foreach (var client in clients)
                    _dispatchQueue.EnqueueEvent(client, eventData);
            }
        }

        /// <inheritdoc />
        public void AddClient(ISubscriptionClientProxy client, SubscriptionEventName eventName)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(eventName, nameof(eventName));
            Validation.ThrowIfNull(client, nameof(client));

            lock (_allclients)
            {
                if (!_allclients.ContainsKey(eventName))
                    _allclients.Add(eventName, new HashSet<SubscriptionClientId>());

                _allclients[eventName].Add(client.Id);
            }
        }

        /// <inheritdoc />
        public void RemoveClient(ISubscriptionClientProxy client, SubscriptionEventName eventName)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            Validation.ThrowIfNull(client, nameof(client));

            lock (_allclients)
            {
                if (_allclients.ContainsKey(eventName))
                {
                    if (_allclients[eventName].Contains(client.Id))
                        _allclients[eventName].Remove(client.Id);
                    if (_allclients[eventName].Count == 0)
                        _allclients.Remove(eventName);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveClient(ISubscriptionClientProxy client)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DefaultSubscriptionEventRouter));

            if (client == null)
                return;

            lock (_allclients)
            {
                var toRemove = new List<SubscriptionEventName>();
                foreach (var kvp in _allclients)
                {
                    if (kvp.Value.Contains(client.Id))
                        kvp.Value.Remove(client.Id);

                    if (kvp.Value.Count == 0)
                        toRemove.Add(kvp.Key);
                }

                // remove any collections that no longer have listeners
                foreach (var key in toRemove)
                    _allclients.Remove(key);
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