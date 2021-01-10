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
    /// An object capable of publishing <see cref="SubscriptionEvent"/> objects to some external
    /// source (like a service bus or message queue) such that they can be received by a
    /// <see cref="ISubscriptionEventRouter"/> and delivered to subscribers.
    /// </summary>
    public interface ISubscriptionEventPublisher
    {
        /// <summary>
        /// Raises a new event in a manner such that a compatible <see cref="ISubscriptionEventRouter" /> could
        /// receive it for processing.
        /// </summary>
        /// <param name="eventData">The event to publish.</param>
        /// <returns>Task.</returns>
        Task PublishEvent(SubscriptionEvent eventData);
    }
}