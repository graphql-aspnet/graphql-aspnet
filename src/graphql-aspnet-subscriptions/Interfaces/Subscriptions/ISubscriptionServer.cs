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
    using GraphQL.AspNet.Interfaces.Messaging;

    /// <summary>
    /// An interface representing a subscription server that can accept events published
    /// by other graphql operations and process them for connected subscribers.
    /// </summary>
    public interface ISubscriptionServer
    {
        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>Task.</returns>
        Task RegisterSubscription(ISubscriptionClientProxy client);

        /// <summary>
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        Task ReceiveEvent(ISubscriptionEvent subscriptionEvent);
    }
}