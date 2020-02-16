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
    /// A proxy object used to publish events to a subscription server.
    /// </summary>
    public interface ISubscriptionEventPublisher
    {
        /// <summary>
        /// Raises a new event in a manner such that a compatible <see cref="ISubscriptionEventPublisher" /> could
        /// receive it for processing.
        /// </summary>
        /// <param name="eventData">The event to publish.</param>
        /// <returns>Task.</returns>
        Task PublishEvent(SubscriptionEvent eventData);
    }
}