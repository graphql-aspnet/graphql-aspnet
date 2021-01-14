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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;

    /// <summary>
    /// The default listener for raised subscription events. This object only listens to the locally attached
    /// graphql server and DOES NOT scale. See demo projects for scalable subscription configurations.
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
            _receivers = new SubscribedEventRecievers(SubscriptionEventNameEqualityComparer.Instance);
        }

        /// <summary>
        /// Forces this listener to raise the given event. May not be invocable by all listeners.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        public async Task RaiseEvent(SubscriptionEvent eventData)
        {
            Validation.ThrowIfNull(eventData, nameof(eventData));

            _logger?.SubscriptionEventReceived(eventData);
            var tasks = new List<Task>();
            lock (_receivers)
            {
                if (_receivers.Count > 0)
                {
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

        /// <summary>
        /// Registers a new receiver to receive any raised events of the given type.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="receiver">The receiver to add.</param>
        public void AddReceiver(SubscriptionEventName eventName, ISubscriptionEventReceiver receiver)
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

        /// <summary>
        /// Removes the receiver from the list of events to be delivered for the given event type.
        /// </summary>
        /// <param name="eventName">Type of the event.</param>
        /// <param name="receiver">The receiver to remove.</param>
        public void RemoveReceiver(SubscriptionEventName eventName, ISubscriptionEventReceiver receiver)
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

        /// <summary>
        /// Removes the receiver from the list of events to be delivered for any event type.
        /// </summary>
        /// <param name="receiver">The receiver to remove.</param>
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