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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions;

    /// <summary>
    /// An object capable of publishing <see cref="SubscriptionEvent"/> objects such that they can be delivered
    /// or received by remote servers.
    /// </summary>
    public interface ISubscriptionEventPublisher
    {
        /// <summary>
        /// Publishes the event in a manner fitting the workflow established by this publisher
        /// such that it can be received by some listener and routed to a server instance
        /// for processing.
        /// </summary>
        /// <param name="eventData">The event to publish.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>ValueTask.</returns>
        ValueTask PublishEvent(SubscriptionEvent eventData, CancellationToken cancelToken = default);
    }
}