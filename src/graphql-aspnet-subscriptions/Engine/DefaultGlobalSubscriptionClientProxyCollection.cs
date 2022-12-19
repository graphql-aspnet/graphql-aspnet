// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System.Collections.Concurrent;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Internal;

    /// <summary>
    /// The default implementation of the collection of all known, active clients
    /// available to this server instance.
    /// </summary>
    internal class DefaultGlobalSubscriptionClientProxyCollection : IGlobalSubscriptionClientProxyCollection
    {
        private readonly ConcurrentDictionary<SubscriptionClientId, ISubscriptionClientProxy> _allClients;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGlobalSubscriptionClientProxyCollection" /> class.
        /// </summary>
        /// <param name="maxConnectedClientCount">The maximum number of clients that can be tracked by this instance.</param>
        public DefaultGlobalSubscriptionClientProxyCollection(int? maxConnectedClientCount)
        {
            _allClients = new ConcurrentDictionary<SubscriptionClientId, ISubscriptionClientProxy>();

            this.MaxAllowedClients = maxConnectedClientCount ?? int.MaxValue;
            if (this.MaxAllowedClients < 0)
                this.MaxAllowedClients = 0;
        }

        /// <inheritdoc />
        public bool TryAddClient(ISubscriptionClientProxy clientProxy)
        {
            Validation.ThrowIfNull(clientProxy, nameof(clientProxy));

            if (this.Count >= this.MaxAllowedClients)
                return false;

            return _allClients.TryAdd(clientProxy.Id, clientProxy);
        }

        /// <inheritdoc />
        public void RemoveClient(ISubscriptionClientProxy clientProxy)
        {
            Validation.ThrowIfNull(clientProxy, nameof(clientProxy));
        }

        /// <inheritdoc />
        public bool TryGetClient(SubscriptionClientId clientId, out ISubscriptionClientProxy clientProxy)
        {
            clientProxy = null;

            if (clientId == null)
                return false;

            return _allClients.TryGetValue(clientId, out clientProxy);
        }

        /// <inheritdoc />
        public int Count => _allClients.Count;

        /// <inheritdoc />
        public int MaxAllowedClients { get; }
    }
}