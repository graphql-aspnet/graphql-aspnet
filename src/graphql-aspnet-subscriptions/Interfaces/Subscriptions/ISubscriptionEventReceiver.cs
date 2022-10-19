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
    /// An object that subscribes to an <see cref="ISubscriptionEventRouter"/>and can receive and asyncronously respond
    /// to an events.
    /// </summary>
    /// <remarks>
    /// This interface should probably be named "subscription event subscriber"
    /// but thats a bit too confusing :).
    /// </remarks>
    public interface ISubscriptionEventReceiver
    {
        /// <summary>
        /// Gets the globally unique id assigned to this instance.
        /// </summary>
        /// <value>The instance's unique id.</value>
        string Id { get; }

        /// <summary>
        /// Called by an outside source, typically an <see cref="ISubscriptionEventRouter"/>,
        /// when an event was raised that this receiver requested.
        /// </summary>
        /// <param name="eventData">The data package representing a raised subscription
        /// event.</param>
        /// <returns>Task.</returns>
        Task ReceiveEvent(SubscriptionEvent eventData);
    }
}