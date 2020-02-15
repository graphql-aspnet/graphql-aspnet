// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// <para>
    /// A subscription server proxy routes events published via <see cref="GraphController"/> to the
    /// subscription server configured for this ASP.NET instance. This proxy class works with a
    /// locally hosted subscription server. As a result, all possiblesubscribed clients are expected
    /// to be connected to the same server instance where this proxy exists.
    /// </para>
    /// <para>
    /// Note: This default implementation can be safely used for a low number of connected clients (typically less than 200).
    /// See the documentation for configuring an external subscription server and scaling support into the 100s and 1000s
    /// connected clients.
    /// </para>
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this proxy is sending events for.</typeparam>
    public class DefaultGraphQLSubscriptionServerProxy<TSchema> : ISubscriptionPublisher<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionServer<TSchema> _subscriptionServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLSubscriptionServerProxy{TSchema}"/> class.
        /// </summary>
        /// <param name="subscriptionServer">A local instance of a subscription server to which created events
        /// should be sent.</param>
        public DefaultGraphQLSubscriptionServerProxy(ISubscriptionServer<TSchema> subscriptionServer)
        {
            _subscriptionServer = subscriptionServer;
        }

        /// <summary>
        /// Raises a new event to the subscription server so that it may send the supplied
        /// data to listening clients.
        /// </summary>
        /// <typeparam name="TData">The type of the data being sent.</typeparam>
        /// <param name="eventName">The schema-unique name of the event.</param>
        /// <param name="dataObject">The data object to send.</param>
        /// <returns>Task.</returns>
        public Task PublishEvent<TData>(string eventName, TData dataObject)
        {
            throw new NotImplementedException();
        }
    }
}