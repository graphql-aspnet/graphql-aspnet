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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The subscription server component is a protocol-agonistic, internal abstraction between the known
    /// subscription-related operations (such as publishing events) and the method through which they are
    /// invoked. Commonly used to swap out in-process vs. out-of-process subscription management. Typically there is
    /// one subscription server instance (a singleton).
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public interface ISubscriptionServer<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Raised when the supervisor begins monitoring a new subscription.
        /// </summary>
        event EventHandler<ClientSubscriptionEventArgs<TSchema>> SubscriptionRegistered;

        /// <summary>
        /// Raised when the supervisor stops monitoring a new subscription.
        /// </summary>
        event EventHandler<ClientSubscriptionEventArgs<TSchema>> SubscriptionRemoved;

        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        void RegisterNewClient(ISubscriptionClientProxy client);

        /// <summary>
        /// Gets the collection of subscriptions this server is managing.
        /// </summary>
        /// <value>The subscriptions.</value>
        ClientSubscriptionCollection<TSchema> Subscriptions { get; }
    }
}