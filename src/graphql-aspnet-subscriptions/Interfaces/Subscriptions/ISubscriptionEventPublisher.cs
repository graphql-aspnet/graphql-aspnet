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
    /// An object capable of publishing <see cref="SubscriptionEvent"/> objects such that they can be delivered
    /// or received by remote servers.
    /// </summary>
    public interface ISubscriptionEventPublisher
    {
        /// <summary>
        /// Raises a new subscription event to a message queue.
        /// </summary>
        /// <param name="eventData">The event to publish.</param>
        /// <returns>Task.</returns>
        Task PublishEvent(SubscriptionEvent eventData);
    }
}