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
    using GraphQL.AspNet.Common;
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
        private ApolloClientSupervisor<TSchema> _supervisor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionServer{TSchema}" /> class.
        /// </summary>
        /// <param name="supervisor">The supervisor in charge of managing client connections for a
        /// given schema.</param>
        public ApolloSubscriptionServer(ApolloClientSupervisor<TSchema> supervisor)
        {
            _supervisor = Validation.ThrowIfNullOrReturn(supervisor, nameof(supervisor));
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
    }
}