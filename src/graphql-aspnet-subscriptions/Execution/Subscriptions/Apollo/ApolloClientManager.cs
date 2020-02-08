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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A client manager (for routing data to clients) based on the apollo graphql over websocket
    /// protocol.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this client manager is registered to handle.</typeparam>
    public class ApolloClientManager<TSchema> : ISubscriptionClientManager<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>Task.</returns>
        public Task RegisterSubscription(ISubscriptionClientProxy client)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Sends the event data to clients.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        public Task SendEventDataToClients(SubscriptionEvent eventData)
        {
            throw new System.NotImplementedException();
        }
    }
}