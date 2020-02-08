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
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface representing a subscription server that can accept events published
    /// by other graphql operations and publish the introspection data for subscription operations
    /// for each schema it is hosting.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public interface ISubscriptionServer<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        Task ReceiveEvent(SubscriptionEvent subscriptionEvent);

        /// <summary>
        /// Retrieves the well known introspection data about the subscription fields supported
        /// by the schema.
        /// </summary>
        /// <returns>Task&lt;SubscriptionOperationDTO&gt;.</returns>
        Task<SubscriptionOperationDTO> RetrieveSubscriptionSchema();
    }
}