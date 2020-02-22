// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A collection of subscriptions filterable by owning client and by subscription event name.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema of the subscriptions this collection is holding..</typeparam>
    public class ClientSubscriptionCollection<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Raised when this collection registers an a subscription of a field type previously unknown to it.
        /// </summary>
        public event ApolloTrackedFieldChangeEventHandler SubscriptionFieldRegistered;

        /// <summary>
        /// Raised when this collection no longer has any active subscriptions for a given field.
        /// </summary>
        public event ApolloTrackedFieldChangeEventHandler SubscriptionFieldAbandoned;

        // a collection of subscriptions keyed by the full path to the field they are targeting.
        private readonly Dictionary<GraphFieldPath, HashSet<ISubscription<TSchema>>> _activeSubscriptionsByRoute;

        private readonly Dictionary<ISubscriptionClientProxy, List<ISubscription<TSchema>>> _activeSubscriptionsByClient;

        private ReaderWriterLockSlim _collectionLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscriptionCollection{TSchema}"/> class.
        /// </summary>
        public ClientSubscriptionCollection()
        {
            _activeSubscriptionsByRoute = new Dictionary<GraphFieldPath, HashSet<ISubscription<TSchema>>>(GraphFieldPathComparer.Instance);
            _activeSubscriptionsByClient = new Dictionary<ISubscriptionClientProxy, List<ISubscription<TSchema>>>();
            _collectionLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Adds a tracked subscription for a given client.
        /// </summary>
        /// <param name="subscription">The subscription to begin tracking.</param>
        public void Add(ISubscription<TSchema> subscription)
        {
            var newlyCreatedEventType = false;

            _collectionLock.EnterWriteLock();

            try
            {
                if (!_activeSubscriptionsByClient.ContainsKey(subscription.Client))
                    _activeSubscriptionsByClient.Add(subscription.Client, new List<ISubscription<TSchema>>());

                if (!_activeSubscriptionsByRoute.ContainsKey(subscription.Field.Route))
                {
                    _activeSubscriptionsByRoute.Add(subscription.Field.Route.Clone(), new HashSet<ISubscription<TSchema>>());
                    newlyCreatedEventType = true;
                }

                _activeSubscriptionsByRoute[subscription.Field.Route].Add(subscription);
                _activeSubscriptionsByClient[subscription.Client].Add(subscription);
            }
            finally
            {
                _collectionLock.ExitWriteLock();
            }

            if (newlyCreatedEventType)
                this.SubscriptionFieldRegistered?.Invoke(this, new ApolloTrackedFieldArgs(subscription.Field));
        }

        /// <summary>
        /// Removes all subscriptions currently tracked for the given client.
        /// </summary>
        /// <param name="client">The client to remove.</param>
        internal void RemoveAllSubscriptions(ISubscriptionClientProxy client)
        {
            _collectionLock.EnterWriteLock();

            var newlyUntrackedEvents = new List<ISubscriptionGraphField>();
            try
            {
                if (_activeSubscriptionsByClient.ContainsKey(client))
                {
                    foreach (var subscription in _activeSubscriptionsByClient[client])
                    {
                        if (_activeSubscriptionsByRoute.ContainsKey(subscription.Field.Route))
                        {
                            _activeSubscriptionsByRoute[subscription.Field.Route].Remove(subscription);
                            if (_activeSubscriptionsByRoute[subscription.Field.Route].Count == 0)
                            {
                                newlyUntrackedEvents.Add(subscription.Field);
                                _activeSubscriptionsByRoute.Remove(subscription.Field.Route);
                            }
                        }
                    }

                    _activeSubscriptionsByClient.Remove(client);
                }
            }
            finally
            {
                _collectionLock.ExitWriteLock();
            }

            foreach (var lostEvent in newlyUntrackedEvents)
            {
                this.SubscriptionFieldAbandoned?.Invoke(this, new ApolloTrackedFieldArgs(lostEvent));
            }
        }

        /// <summary>
        /// Determines whether this instance contains a subscription for the given client with the provided id.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the client has already registered the given id; otherwise, <c>false</c>.</returns>
        public bool Contains(ISubscriptionClientProxy client, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || client == null)
                return false;

            if (!_activeSubscriptionsByClient.ContainsKey(client))
                return false;

            return _activeSubscriptionsByClient[client].Any(x => x.ClientProvidedId == id);
        }

        /// <summary>
        /// Attempts to remove a subscription for a given client based on the client supplied subscription id. If found, the subscription
        /// is removed from the collection and returned.  Null is returned if the subscription or the client is not found.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="clientProvidedId">The original client supplied identifier for the subscription.</param>
        /// <returns>ClientSubscription&lt;TSchema&gt;.</returns>
        public ISubscription<TSchema> TryRemoveSubscription(ISubscriptionClientProxy client, string clientProvidedId)
        {
            _collectionLock.EnterWriteLock();

            bool eventCleared = false;
            ISubscription<TSchema> subscription = null;
            try
            {
                if (!_activeSubscriptionsByClient.ContainsKey(client))
                    return null;

                subscription = _activeSubscriptionsByClient[client].FirstOrDefault(x => x.ClientProvidedId == clientProvidedId);
                if (subscription == null)
                    return null;

                _activeSubscriptionsByClient[client].Remove(subscription);
                if (_activeSubscriptionsByRoute.ContainsKey(subscription.Field.Route))
                {
                    _activeSubscriptionsByRoute[subscription.Field.Route].Remove(subscription);
                    if (_activeSubscriptionsByRoute[subscription.Field.Route].Count == 0)
                    {
                        eventCleared = true;
                        _activeSubscriptionsByRoute.Remove(subscription.Field.Route);
                    }
                }

                if (_activeSubscriptionsByClient[client].Count == 0)
                    _activeSubscriptionsByClient.Remove(client);
            }
            finally
            {
                _collectionLock.ExitWriteLock();
            }

            if (eventCleared && subscription != null)
                this.SubscriptionFieldAbandoned?.Invoke(this, new ApolloTrackedFieldArgs(subscription.Field));

            return subscription;
        }

        /// <summary>
        /// Retrieves all the active subscriptions of a given eventName across all registered clients.
        /// </summary>
        /// <param name="fieldPath">The field path.</param>
        /// <returns>IEnumerable&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ISubscription<TSchema>> RetrieveSubscriptions(GraphFieldPath fieldPath)
        {
            if (fieldPath == null)
                return Enumerable.Empty<ISubscription<TSchema>>();

            _collectionLock.EnterReadLock();

            try
            {
                if (_activeSubscriptionsByRoute.ContainsKey(fieldPath))
                    return _activeSubscriptionsByRoute[fieldPath];

                return Enumerable.Empty<ISubscription<TSchema>>();
            }
            finally
            {
                _collectionLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Retrieves the subscriptions currently active for a given client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>IEnumerable&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ISubscription<TSchema>> RetrieveSubscriptions(ISubscriptionClientProxy client)
        {
            _collectionLock.EnterReadLock();
            try
            {
                if (!_activeSubscriptionsByClient.ContainsKey(client))
                    return Enumerable.Empty<ISubscription<TSchema>>();

                return _activeSubscriptionsByClient[client];
            }
            finally
            {
                _collectionLock.ExitReadLock();
            }
        }
    }
}