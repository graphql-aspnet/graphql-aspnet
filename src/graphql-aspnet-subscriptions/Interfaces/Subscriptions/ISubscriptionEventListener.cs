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

    /// <summary>
    /// An interface describing a mechanism for a listener to recieve new instructions
    /// about the events it should be listening for.
    /// </summary>
    public interface ISubscriptionEventListener
    {
        /// <summary>
        /// Forces this listener to raise the given event. May not be invocable by all listeners.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        Task RaiseEvent(SubscriptionEvent eventData);

        /// <summary>
        /// Registers a new receiver to receive any raised events of the given type.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="receiver">The receiver to add.</param>
        void AddReceiver(SubscriptionEventName eventName, ISubscriptionEventReceiver receiver);

        /// <summary>
        /// Removes the receiver from the list of events to be delivered for the given event type.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="receiver">The receiver to remove.</param>
        void RemoveReceiver(SubscriptionEventName eventName, ISubscriptionEventReceiver receiver);

         /// <summary>
        /// Removes the receiver from the list of events to be delivered for any event type.
        /// </summary>
        /// <param name="receiver">The receiver to remove.</param>
        void RemoveReceiver(ISubscriptionEventReceiver receiver);
    }
}