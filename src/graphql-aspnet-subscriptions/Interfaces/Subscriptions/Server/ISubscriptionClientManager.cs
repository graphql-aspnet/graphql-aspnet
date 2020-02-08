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
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The subscription manager coordinates connected clients via their <see cref="ISubscriptionServer{TSchema}" />
    /// and routes recieved events to the appropriate clients as needed.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this client manager is registered to handle.</typeparam>
    public interface ISubscriptionClientManager<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>Task.</returns>
        Task RegisterSubscription(ISubscriptionClientProxy client);

        /// <summary>
        /// Sends the event data to clients.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        Task SendEventDataToClients(SubscriptionEvent eventData);
    }
}