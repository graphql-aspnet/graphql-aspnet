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
    /// An interface describing an object for receiving events from a given source and routing them to
    /// the correct recievers that should handle said events. This object is typically called by
    /// an external, platform dependent listener (such as a service bus client) and is
    /// used to route deserialized events into the graphql subscription server instance present for each
    /// declared schema. This object is schema agnostic and should be registered as a global
    /// singleton in a DI container.
    /// </summary>
    public interface ISubscriptionEventRouter
    {
        /// <summary>
        /// Instructs this router to raise the supplied event to each subscribed receiver.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        Task RaiseEvent(SubscriptionEvent eventData);

        /// <summary>
        /// Registers a new receiver to receive any raised events of the given type seen
        /// by this listener.
        /// </summary>
        /// <param name="receiver">The receiver to add.</param>
        /// <param name="eventName">Name of the event to listen for.</param>
        void AddReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName);

        /// <summary>
        /// Removes the receiver from being notified of the given event type.
        /// </summary>
        /// <param name="receiver">The receiver to remove.</param>
        /// <param name="eventName">Name of the event to stop listening for.</param>
        void RemoveReceiver(ISubscriptionEventReceiver receiver, SubscriptionEventName eventName);

         /// <summary>
        /// Removes the receiver from being notified of ALL events that it may be registered to.
        /// </summary>
        /// <param name="receiver">The receiver to remove.</param>
        void RemoveReceiver(ISubscriptionEventReceiver receiver);
    }
}