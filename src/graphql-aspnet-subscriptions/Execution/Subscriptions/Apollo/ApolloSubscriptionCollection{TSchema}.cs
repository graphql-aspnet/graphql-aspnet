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
    using System.Collections.Generic;
    using System.Threading;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection of subscriptions filterable by owning client and subscription type.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema of the subscriptions this collection is holding..</typeparam>
    public class ApolloSubscriptionCollection<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Raised when this collection registers an event of a type not previously known to it.
        /// </summary>
        public event ApolloTrackedEventChangeEventHandler EventRegistered;

        /// <summary>
        /// Raised when this collection no longer has any active subscriptions for a given event type.
        /// </summary>
        public event ApolloTrackedEventChangeEventHandler EventAbandonded;

        // a collection of subscriptions this supervisor is watching for
        // keyed on teh full field path within the target schema (not against the alternate, short event name)
        private readonly Dictionary<string, HashSet<ClientSubscription<TSchema>>> _activeSubscriptionsByEvent;
        private readonly Dictionary<ISubscriptionClientProxy, HashSet<ClientSubscription<TSchema>>> _activeSubscriptionsByClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionCollection{TSchema}"/> class.
        /// </summary>
        public ApolloSubscriptionCollection()
        {
            _activeSubscriptionsByEvent = new Dictionary<string, HashSet<ClientSubscription<TSchema>>>();
            _activeSubscriptionsByClient = new Dictionary<ISubscriptionClientProxy, HashSet<ClientSubscription<TSchema>>>();
        }

        /// <summary>
        /// Adds a tracked subscription for a given client.
        /// </summary>
        /// <param name="subscription">The subscription to begin tracking.</param>
        public void Add(ClientSubscription<TSchema> subscription)
        {
            bool newlyCreatedEventType = false;
            Monitor.Enter(this);

            if (!_activeSubscriptionsByClient.ContainsKey(subscription.Client))
                _activeSubscriptionsByClient.Add(subscription.Client, new HashSet<ClientSubscription<TSchema>>());

            if (!_activeSubscriptionsByEvent.ContainsKey(subscription.Field.Route.Path))
            {
                _activeSubscriptionsByEvent.Add(subscription.Field.Route.Path, new HashSet<ClientSubscription<TSchema>>());
                newlyCreatedEventType = true;
            }

            _activeSubscriptionsByEvent[subscription.Field.Route.Path].Add(subscription);
            _activeSubscriptionsByClient[subscription.Client].Add(subscription);

            Monitor.Exit(this);

            if (newlyCreatedEventType)
                this.EventRegistered?.Invoke(this, new ApolloTrackedEventArgs(subscription.Field));
        }

        /// <summary>
        /// Removes all subscriptions currently tracked for the given client.
        /// </summary>
        /// <param name="client">The client to remove.</param>
        internal void RemoveAllSubscriptions(ISubscriptionClientProxy client)
        {
            Monitor.Enter(this);

            var newlyUntrackedEvents = new List<ISubscriptionGraphField>();
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

            Monitor.Exit(this);

            foreach (var lostEvent in newlyUntrackedEvents)
                this.EventAbandonded?.Invoke(this, new ApolloTrackedEventArgs(lostEvent));
        }

        /// <summary>
        /// Retrieves all the active subscriptions of a given eventName across all registered clients.
        /// </summary>
        /// <param name="eventName">The fully qualified event name.</param>
        /// <returns>IEnumerable&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ClientSubscription<TSchema>> RetrieveSubscriptions(string eventName)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Retrieves the subscriptions currently active for a given client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>IEnumerable&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ClientSubscription<TSchema>> RetrieveSubscriptions(ISubscriptionClientProxy client)
        {
            throw new NotImplementedException();
        }
    }
}