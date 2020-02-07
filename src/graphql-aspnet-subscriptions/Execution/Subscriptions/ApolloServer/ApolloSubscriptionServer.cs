// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.ApolloServer
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A subscription server capable of filtering and sending new data to
    /// subscribed clients using the apollo graphql messaging protocol.
    /// </summary>
    public class ApolloSubscriptionServer : ISubscriptionServer
    {
        /// <summary>
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        public Task ReceiveEvent(ISubscriptionEvent subscriptionEvent)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>Task.</returns>
        public Task RegisterSubscription(ISubscriptionClientProxy client)
        {
            throw new System.NotImplementedException();
        }
    }
}