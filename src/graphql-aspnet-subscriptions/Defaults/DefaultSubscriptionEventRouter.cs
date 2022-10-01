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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;

    /// <summary>
    /// The default listener for raised subscription events. This object routes recieved subscription
    /// events to the various schema instances  within this application domain. This component IS NOT
    /// responsible for publishing new events, only receieving existing ones.
    /// </summary>
    public class DefaultSubscriptionEventRouter : ISubscriptionEventRouter
    {
        private readonly IGraphEventLogger _logger;
        private readonly SubscribedEventRecievers _allReceivers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionEventRouter" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultSubscriptionEventRouter(IGraphEventLogger logger = null)
        {
            _logger = logger;
            _allReceivers = new SubscribedEventRecievers();
        }

        /// <inheritdoc />
        public async Task RaisePublishedEvent(SubscriptionEvent eventData)
        {
            Validation.ThrowIfNull(eventData, nameof(eventData));

            _logger?.SubscriptionEventReceived(eventData);
            List<ISubscriptionEventReceiver> receivers = null;

            var tasks = new List<Task>();
            lock (_allReceivers)
            {
                // if no one is listening for the event, just let it go
                var eventName = eventData.ToSubscriptionEventName();
                if (!_allReceivers.ContainsKey(eventName))
                    return;

                receivers = new List<ISubscriptionEventReceiver>(_allReceivers[eventName].Count);
                receivers.AddRange(_allReceivers[eventName]);
            }

            if (receivers != null)
            {
                await Task.Yield();
                foreach (var receiver in receivers)
                {
                    var task = receiver.ReceiveEvent(eventData);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
        }

        /// <inheritdoc />
        public void AddReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName)
        {
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
    }
}