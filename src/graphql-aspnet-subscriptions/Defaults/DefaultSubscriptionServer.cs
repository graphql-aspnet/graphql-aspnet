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
    /// A subscription server capable of filtering and sending new data to
    /// subscribed clients using the apollo graphql messaging protocol.
    /// </summary>
    /// <typeparam name="TSchema">The schema type to retrieve subscription introspection data.</typeparam>
    public class DefaultSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        public Task ReceiveEvent(SubscriptionEvent subscriptionEvent)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Retrieves the well known introspection data about the subscription fields supported
        /// by the schema.
        /// </summary>
        /// <returns>Task&lt;SubscriptionOperationDTO&gt;.</returns>
        public Task<SubscriptionOperationDTO> RetrieveSubscriptionSchema()
        {
            throw new System.NotImplementedException();
        }
    }
}