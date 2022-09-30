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
        private readonly SubscribedEventRecievers _receivers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionEventRouter" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultSubscriptionEventRouter(IGraphEventLogger logger = null)
        {
            _logger = logger;
            _receivers = new SubscribedEventRecievers();
        }

        /// <inheritdoc />
        public async Task RaiseEvent(SubscriptionEvent eventData)
        {
            Validation.ThrowIfNull(eventData, nameof(eventData));

            _logger?.SubscriptionEventReceived(eventData);
            var tasks = new List<Task>();
            lock (_receivers)
            {
                if (_receivers.Count > 0)
                {
                    // if no one is listening for the event, just let it go
                    var eventName = eventData.ToSubscriptionEventName();
                    if (!_receivers.ContainsKey(eventName))
                        return;

                    foreach (var receiver in _receivers[eventName])
                    {
                        var task = receiver.ReceiveEvent(eventData);
                        tasks.Add(task);
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public void AddReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName)
        {
            Validation.ThrowIfNull(eventName, nameof(eventName));
            Validation.ThrowIfNull(receiver, nameof(receiver));

            lock (_receivers)
            {
                if (!_receivers.ContainsKey(eventName))
                    _receivers.Add(eventName, new HashSet<ISubscriptionEventReceiver>());

                _receivers[eventName].Add(receiver);
            }
        }

        /// <inheritdoc />
        public void RemoveReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName)
        {
            if (receiver == null || eventName == null)
                return;

            lock (_receivers)
            {
                if (_receivers.ContainsKey(eventName))
                {
                    if (_receivers[eventName].Contains(receiver))
                        _receivers[eventName].Remove(receiver);
                    if (_receivers[eventName].Count == 0)
                        _receivers.Remove(eventName);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveReceiver(ISubscriptionEventReceiver receiver)
        {
            if (receiver == null)
                return;

            lock (_receivers)
            {
                var toRemove = new List<SubscriptionEventName>();
                foreach (var kvp in _receivers)
                {
                    if (kvp.Value.Contains(receiver))
                        kvp.Value.Remove(receiver);

                    if (kvp.Value.Count == 0)
                        toRemove.Add(kvp.Key);
                }

                // remove any collections that no longer have listeners
                foreach (var key in toRemove)
                    _receivers.Remove(key);
            }
        }
    }
}