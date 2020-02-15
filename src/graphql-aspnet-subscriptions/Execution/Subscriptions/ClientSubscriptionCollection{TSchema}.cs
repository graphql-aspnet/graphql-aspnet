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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection of subscriptions filterable by owning client and by subscription event name.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema of the subscriptions this collection is holding..</typeparam>
    public class ClientSubscriptionCollection<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Raised when this collection registers an event of a type not previously known to it.
        /// </summary>
        public event ApolloTrackedEventChangeEventHandler EventRegistered;

        /// <summary>
        /// Raised when this collection no longer has any active subscriptions for a given event type.
        /// </summary>
        public event ApolloTrackedEventChangeEventHandler EventAbandoned;

        // a collection of subscriptions this supervisor is watching for
        // keyed on teh full field path within the target schema (not against the alternate, short event name)
        private readonly Dictionary<string, HashSet<ISubscription<TSchema>>> _activeSubscriptionsByEvent;
        private readonly Dictionary<ISubscriptionClientProxy, List<ISubscription<TSchema>>> _activeSubscriptionsByClient;

        private ReaderWriterLockSlim _collectionLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscriptionCollection{TSchema}"/> class.
        /// </summary>
        public ClientSubscriptionCollection()
        {
            _activeSubscriptionsByEvent = new Dictionary<string, HashSet<ISubscription<TSchema>>>();
            _activeSubscriptionsByClient = new Dictionary<ISubscriptionClientProxy, List<ISubscription<TSchema>>>();
            _collectionLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Adds a tracked subscription for a given client.
        /// </summary>
        /// <param name="subscription">The subscription to begin tracking.</param>
        public void Add(ISubscription<TSchema> subscription)
        {
            bool newlyCreatedEventType = false;

            _collectionLock.EnterWriteLock();

            try
            {
                if (!_activeSubscriptionsByClient.ContainsKey(subscription.Client))
                    _activeSubscriptionsByClient.Add(subscription.Client, new List<ISubscription<TSchema>>());

                if (!_activeSubscriptionsByEvent.ContainsKey(subscription.Field.Route.Path))
                {
                    _activeSubscriptionsByEvent.Add(subscription.Field.Route.Path, new HashSet<ISubscription<TSchema>>());
                    newlyCreatedEventType = true;
                }

                _activeSubscriptionsByEvent[subscription.Field.Route.Path].Add(subscription);
                _activeSubscriptionsByClient[subscription.Client].Add(subscription);
            }
            finally
            {
                _collectionLock.ExitWriteLock();
            }

            if (newlyCreatedEventType)
                this.EventRegistered?.Invoke(this, new ApolloTrackedEventArgs(subscription.Field));
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
                        if (_activeSubscriptionsByEvent.ContainsKey(subscription.Field.Route.Path))
                        {
                            _activeSubscriptionsByEvent[subscription.Field.Route.Path].Remove(subscription);
                            if (_activeSubscriptionsByEvent[subscription.Field.Route.Path].Count == 0)
                            {
                                newlyUntrackedEvents.Add(subscription.Field);
                                _activeSubscriptionsByEvent.Remove(subscription.Field.Route.Path);
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
                this.EventAbandoned?.Invoke(this, new ApolloTrackedEventArgs(lostEvent));
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
                if (_activeSubscriptionsByEvent.ContainsKey(subscription.Field.Route.Path))
                {
                    _activeSubscriptionsByEvent[subscription.Field.Route.Path].Remove(subscription);
                    if (_activeSubscriptionsByEvent[subscription.Field.Route.Path].Count == 0)
                    {
                        eventCleared = true;
                        _activeSubscriptionsByEvent.Remove(subscription.Field.Route.Path);
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
                this.EventAbandoned?.Invoke(this, new ApolloTrackedEventArgs(subscription.Field));

            return subscription;
        }

        /// <summary>
        /// Retrieves all the active subscriptions of a given eventName across all registered clients.
        /// </summary>
        /// <param name="eventName">The fully qualified event name.</param>
        /// <returns>IEnumerable&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ISubscription<TSchema>> RetrieveSubscriptions(string eventName)
        {
            _collectionLock.EnterReadLock();

            try
            {
                if (!_activeSubscriptionsByEvent.ContainsKey(eventName))
                    return Enumerable.Empty<ISubscription<TSchema>>();

                return _activeSubscriptionsByEvent[eventName];
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