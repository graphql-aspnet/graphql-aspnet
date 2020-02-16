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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// The default listener for raised subscription events. This object only listens to the locally attached
    /// graphql server and DOES NOT scale. See demo projects for scalable subscription configurations.
    /// </summary>
    public class InProcessSubscriptionEventListener : ISubscriptionEventListener
    {
        private Dictionary<string, HashSet<ISubscriptionEventReceiver>> _receivers;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcessSubscriptionEventListener"/> class.
        /// </summary>
        public InProcessSubscriptionEventListener()
        {
            _receivers = new Dictionary<string, HashSet<ISubscriptionEventReceiver>>();
        }

        /// <summary>
        /// Forces this listener to raise the given event. May not be invocable by all listeners.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        public async Task RaiseEvent(SubscriptionEvent eventData)
        {
            Validation.ThrowIfNull(eventData, nameof(eventData));

            var tasks = new List<Task>();
            lock (_receivers)
            {
                var eventString = $"{eventData.SchemaTypeName}:{eventData.EventName}";
                if (!_receivers.ContainsKey(eventString))
                    return;

                foreach (var receiver in _receivers[eventString])
                {
                    var task = receiver.ReceiveEvent(eventData);
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Registers a new receiver to receive any raised events of the given type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="receiver">The receiver to add.</param>
        public void AddReceiver(IMonitoredSubscriptionEvent eventType, ISubscriptionEventReceiver receiver)
        {
            Validation.ThrowIfNull(eventType, nameof(eventType));
            Validation.ThrowIfNull(receiver, nameof(receiver));

            lock (_receivers)
            {
                var eventString = $"{eventType.SchemaType.FullName}:{eventType.Route.Path}";
                if (!_receivers.ContainsKey(eventString))
                    _receivers.Add(eventString, new HashSet<ISubscriptionEventReceiver>());

                _receivers[eventString].Add(receiver);

                if (!string.IsNullOrWhiteSpace(eventType.EventName))
                {
                    eventString = $"{eventType.EventName}:{eventType.Route.Path}";
                    if (!_receivers.ContainsKey(eventString))
                        _receivers.Add(eventString, new HashSet<ISubscriptionEventReceiver>());

                    _receivers[eventString].Add(receiver);
                }
            }
        }

        /// <summary>
        /// Removes the receiver from the list of events to be delivered for the given event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="receiver">The receiver to remove.</param>
        public void RemoveReceiver(IMonitoredSubscriptionEvent eventType, ISubscriptionEventReceiver receiver)
        {
            if (receiver == null)
                return;

            lock (_receivers)
            {
                var eventString = $"{eventType.SchemaType.FullName}:{eventType.Route.Path}";
                if (_receivers.ContainsKey(eventString))
                {
                    if (_receivers[eventString].Contains(receiver))
                        _receivers[eventString].Remove(receiver);
                    if (_receivers[eventString].Count == 0)
                        _receivers.Remove(eventString);
                }

                if (!string.IsNullOrWhiteSpace(eventType.EventName))
                {
                    eventString = $"{eventType.SchemaType.FullName}:{eventType.EventName}";

                    if (_receivers[eventString].Contains(receiver))
                        _receivers[eventString].Remove(receiver);
                    if (_receivers[eventString].Count == 0)
                        _receivers.Remove(eventString);
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
                var toRemove = new List<string>();
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