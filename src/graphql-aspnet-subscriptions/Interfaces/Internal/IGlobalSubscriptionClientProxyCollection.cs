﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.SubscriptionServer;

    /// <summary>
    /// A collection of all known, active subscription clients attached to this
    /// server instance.
    /// </summary>
    public interface IGlobalSubscriptionClientProxyCollection
    {
        /// <summary>
        /// Attempts to add the client to the collection. If the collection
        /// is full the client will be rejected.
        /// </summary>
        /// <param name="clientProxy">The client proxy to add to this collection.</param>
        /// <returns><c>true</c> if the client was successfully registered;
        /// otherwise, <c>false</c>.</returns>
        bool TryAddClient(ISubscriptionClientProxy clientProxy);

        /// <summary>
        /// Attempts to remove the client from the client. If it is not
        /// part of the collection, the operation is ignored.
        /// </summary>
        /// <param name="clientId">A client identifier.</param>
        /// <param name="clientProxy">The client proxy that was removed, if found, otherwise null.</param>
        /// <returns><c>true</c> if the client was removed from the collection, <c>false</c> otherwise.</returns>
        bool TryRemoveClient(SubscriptionClientId clientId, out ISubscriptionClientProxy clientProxy);

        /// <summary>
        /// Tries to retrieve a client with the given id.
        /// </summary>
        /// <param name="clientId">A client identifier.</param>
        /// <param name="clientProxy">When the <paramref name="clientId"/> is found,
        /// this parameter will be filled with the appropriate proxy instance.</param>
        /// <returns><c>true</c> if the client was found in the collection;
        /// otherwise, <c>false</c>.</returns>
        bool TryGetClient(SubscriptionClientId clientId, out ISubscriptionClientProxy clientProxy);

        /// <summary>
        /// Gets the current number of registered clients.
        /// </summary>
        /// <value>The number of actively tracked clients.</value>
        int Count { get; }

        /// <summary>
        /// Gets the maximum number of allowed clients this instance will
        /// support.
        /// </summary>
        /// <value>The maximum allowed number of clients.</value>
        public int MaxAllowedClients { get; }
    }
}