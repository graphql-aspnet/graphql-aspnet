// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface describing a mechanism for a listener to recieve new instructions
    /// about the events it should be listening for.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public interface ISubscriptionEventListener<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// An event raised whenever this listener recieves a new event from the source
        /// its monitoring.
        /// </summary>
        event SubscriptionEventHandler NewSubscriptionEvent;

        /// <summary>
        /// Forces this listener to raise the given event. May not be invocable by all listeners.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        Task RaiseEvent(SubscriptionEvent eventData);

        /// <summary>
        /// Adds the event name to the list of events this listener should listen for. The listener
        /// will begin processing events of this type.
        /// </summary>
        /// <param name="name">The fully qualified name of the event.</param>
        void AddEventType(string name);

        /// <summary>
        /// Removes the event name from the list of events this listener should listen for. No more
        /// events of this type will be processed.
        /// </summary>
        /// <param name="name">The fully qualified name of the event.</param>
        void RemoveEventType(string name);
    }
}