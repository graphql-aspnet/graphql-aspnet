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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The default listener for raised subscription events. This object only listens to the locally attached
    /// graphql server and DOES NOT scale. See demo projects for scalable subscription configurations.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this listener is invoked for.</typeparam>
    public class InProcessSubscriptionEventListener<TSchema> : ISubscriptionEventListener<TSchema>
        where TSchema : class, ISchema
    {
        private ConcurrentHashSet<string> _monitoredEvents;
        private HashSet<ISubscriptionEventReceiver> _receivers;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcessSubscriptionEventListener{TSchema}"/> class.
        /// </summary>
        public InProcessSubscriptionEventListener()
        {
            _monitoredEvents = new ConcurrentHashSet<string>();
            _receivers = new HashSet<ISubscriptionEventReceiver>();
        }

        /// <summary>
        /// Forces this listener to raise the given event. May not be invocable by all listeners.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        public async Task RaiseEvent(SubscriptionEvent eventData)
        {
            Validation.ThrowIfNull(eventData, nameof(eventData));

            if (!_monitoredEvents.Contains(eventData.EventName))
                return;

            if (eventData.SchemaTypeName != typeof(TSchema).FullName)
            {
                throw new InvalidOperationException($"The event listener, '{this.GetType().FriendlyName()}', can only raise events " +
                    $"for the schema '{typeof(TSchema).FriendlyName()}' (Event Schema: {eventData.SchemaTypeName}).");
            }

            var tasks = new List<Task>();
            foreach (var receiver in _receivers)
            {
                var task = receiver.ReceiveEvent(eventData);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Adds the event name to the list of events this listener should listen for. The listener
        /// will begin processing events of this type.
        /// </summary>
        /// <param name="name">The fully qualified name of the event.</param>
        public void AddEventType(string name)
        {
            _monitoredEvents.Add(name);
        }

        /// <summary>
        /// Removes the event name from the list of events this listener should listen for. No more
        /// events of this type will be processed.
        /// </summary>
        /// <param name="name">The fully qualified name of the event.</param>
        public void RemoveEventType(string name)
        {
            _monitoredEvents.TryRemove(name);
        }

        /// <summary>
        /// Registers a new receiver to forward any raised events to.
        /// </summary>
        /// <param name="receiver">The receiver to add.</param>
        public void AddReceiver(ISubscriptionEventReceiver receiver)
        {
            if (receiver == null)
                return;

            lock (_receivers)
                _receivers.Add(receiver);
        }

        /// <summary>
        /// Removes the receiver from the list of objects to receive raised events.
        /// </summary>
        /// <param name="receiver">The receiver to remove.</param>
        public void RemoveReceiver(ISubscriptionEventReceiver receiver)
        {
            if (receiver == null)
                return;

            lock (_receivers)
                _receivers.Remove(receiver);
        }
    }
}