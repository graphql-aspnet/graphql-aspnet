// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Messaging;

    /// <summary>
    /// Represents a client that has subscribed to a given event and is ready to receive data.
    /// data.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema hosting this subscription.</typeparam>
    public class ApolloSubscription<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscription{TSchema}"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ApolloSubscription(ApolloClientProxy<TSchema> client)
        {
            this.Client = Validation.ThrowIfNullOrReturn(client, nameof(client));
        }

        /// <summary>
        /// Gets the client that created this subscription.
        /// </summary>
        /// <value>The client.</value>
        public ApolloClientProxy<TSchema> Client { get; }
    }
}