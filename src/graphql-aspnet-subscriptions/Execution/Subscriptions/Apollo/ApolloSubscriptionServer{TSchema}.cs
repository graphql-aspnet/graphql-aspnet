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
    using GraphQL.AspNet.Messaging;

    /// <summary>
    /// A baseline component acts to centralize the subscription server operations, regardless of if
    /// this server is in-process with the primary graphql runtime or out-of-process on a seperate instance.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public class ApolloSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>
        where TSchema : class, ISchema
    {
        private ApolloClientManager<TSchema> _clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionServer{TSchema}"/> class.
        /// </summary>
        public ApolloSubscriptionServer()
        {
            _clientManager = new ApolloClientManager<TSchema>();
        }

        /// <summary>
        /// Registers a newly connected client with the server.
        /// </summary>
        /// <param name="client">The client.</param>
        public void RegisterClient(ISubscriptionClientProxy client)
        {
            _clientManager.RegisterClient(client);
        }

        /// <summary>
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <typeparam name="TData">The type of the data being recieved on the event.</typeparam>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        public Task PublishEvent<TData>(SubscriptionEvent<TData> subscriptionEvent)
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