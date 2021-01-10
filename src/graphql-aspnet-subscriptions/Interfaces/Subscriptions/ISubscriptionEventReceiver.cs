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
    /// An object that can receive and asyncronously respond to an event being raised by a listener.
    /// This object is generally used by a sever component to correctly translate externally
    /// received events into a "server-specific" format.
    /// </summary>
    public interface ISubscriptionEventReceiver
    {
        /// <summary>
        /// Receives a new event that was seen by a listener.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        Task ReceiveEvent(SubscriptionEvent eventData);
    }
}