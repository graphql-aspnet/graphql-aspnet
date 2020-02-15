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

        /// <summary>
        /// An event raised whenever this listener recieves a new event from the source
        /// its monitoring.
        /// </summary>
        public event SubscriptionEventHandler NewSubscriptionEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcessSubscriptionEventListener{TSchema}"/> class.
        /// </summary>
        public InProcessSubscriptionEventListener()
        {
            _monitoredEvents = new ConcurrentHashSet<string>();
        }

        /// <summary>
        /// Forces this listener to raise the given event. May not be invocable by all listeners.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        public Task RaiseEvent(SubscriptionEvent eventData)
        {
            Validation.ThrowIfNull(eventData, nameof(eventData));
            if (eventData.SchemaTypeName != typeof(TSchema).FullName)
            {
                throw new InvalidOperationException($"The event listener, '{this.GetType().FriendlyName()}', can only raise events " +
                    $"for the schema '{typeof(TSchema).FriendlyName()}' (Event Schema: {eventData.SchemaTypeName}).");
            }

            this.NewSubscriptionEvent?.Invoke(this, new SubscriptionEventEventArgs(eventData));
            return Task.CompletedTask;
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
    }
}