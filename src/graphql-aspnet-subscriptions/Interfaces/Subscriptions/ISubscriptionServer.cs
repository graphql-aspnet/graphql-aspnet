﻿// *************************************************************
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
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <typeparam name="TData">The type of the data being recieved on the event.</typeparam>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        Task PublishEvent<TData>(SubscriptionEvent<TData> subscriptionEvent);
    }
}