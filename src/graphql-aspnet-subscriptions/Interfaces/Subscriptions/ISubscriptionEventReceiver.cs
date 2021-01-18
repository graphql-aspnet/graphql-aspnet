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
    /// <para>
    /// An object that subscribes to a <see cref="ISubscriptionEventRouter"/>and can receive and asyncronously respond
    /// to an events. This object is generally used by a subscription sever component to correctly translate externally
    /// received events into a "server-specific" format.
    /// </para>
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