// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A collection of subscriptions tracked by a single client.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    internal class ApolloSubscriptionCollection<TSchema> : IEnumerable<ISubscription<TSchema>>
        where TSchema : class, ISchema
    {
        private object _syncLock = new object();
        private Dictionary<string, ISubscription<TSchema>> _subsById;
        private Dictionary<GraphFieldPath,  HashSet<ISubscription<TSchema>>> _subsByRoute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionCollection{TSchema}"/> class.
        /// </summary>
        public ApolloSubscriptionCollection()
        {
            _subsById = new Dictionary<string, ISubscription<TSchema>>();
            _subsByRoute = new Dictionary<GraphFieldPath, HashSet<ISubscription<TSchema>>>(GraphFieldPathComparer.Instance);
        }

        /// <summary>
        /// Adds the specified subscription to the collection of tracked subscriptions and returns
        /// the total number of tracked subscriptions for the subscriptions route.
        /// </summary>
        /// <param name="subscription">The subscription to add.</param>
        /// <returns>The total number of subscriptions registered for the route of the new subscription.</returns>
        public int Add(ISubscription<TSchema> subscription)
        {
            Validation.ThrowIfNullOrReturn(subscription, nameof(subscription));

            lock (_syncLock)
            {
                if (_subsById.ContainsKey(subscription.Id))
                {
                    throw new ArgumentException($"The subscription id '{subscription.Id}' is already" +
                          $"being tracked by this collection");
                }

                _subsById.Add(subscription.Id, subscription);

                if (!_subsByRoute.ContainsKey(subscription.Route))
                    _subsByRoute.Add(subscription.Route, new HashSet<ISubscription<TSchema>>());

                _subsByRoute[subscription.Route].Add(subscription);

                return _subsByRoute[subscription.Route].Count;
            }
        }

        /// <summary>
        /// Removes the specified subscription from the tracked collection and gives a
        /// reference to it. If no subscription is found with the given id, null is set. This
        /// method also returns the total number of subscriptions for the found subscriptions route
        /// that still remain tracked.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to remove.</param>
        /// <param name="removedSub">A reference to the found subscription that was removed from the
        /// collection, null if no subscription was found.</param>
        /// <returns>The total number of remaining subscriptions for the route of the found subscription.</returns>
        public int Remove(string subscriptionId, out ISubscription<TSchema> removedSub)
        {
            removedSub = null;
            if (string.IsNullOrWhiteSpace(subscriptionId))
                return 0;

            int remaining = 0;
            lock (_syncLock)
            {
                if (_subsById.ContainsKey(subscriptionId))
                {
                    removedSub = _subsById[subscriptionId];
                    _subsById.Remove(subscriptionId);
                }

                if (removedSub != null && _subsByRoute.ContainsKey(removedSub.Route))
                {
                    var hashSet = _subsByRoute[removedSub.Route];
                    if (hashSet.Contains(removedSub))
                        hashSet.Remove(removedSub);

                    remaining = hashSet.Count;
                    if (remaining == 0)
                        _subsByRoute.Remove(removedSub.Route);
                }
            }

            return remaining;
        }

        /// <summary>
        /// Retrieves the total number of subscriptions registered for the given unique route.
        /// </summary>
        /// <param name="route">The route to filter by.</param>
        /// <returns>System.Int32.</returns>
        public int CountByRoute(GraphFieldPath route)
        {
            if (route == null || !route.IsValid)
                return 0;

            lock (_syncLock)
            {
                if (!_subsByRoute.ContainsKey(route))
                    return 0;

                return _subsByRoute[route].Count;
            }
        }

        /// <summary>
        /// Finds the set all known subscriptions for a given route and returns them.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>IEnumerable&lt;ISubscription&lt;TSchema&gt;&gt;.</returns>
        public IReadOnlyList<ISubscription<TSchema>> RetreiveByRoute(GraphFieldPath route)
        {
            List<ISubscription<TSchema>> subs = new List<ISubscription<TSchema>>();
            if (route != null)
            {
                lock (_syncLock)
                {
                    if (_subsByRoute.ContainsKey(route))
                    {
                        subs.AddRange(_subsByRoute[route]);
                    }
                }
            }

            return subs;
        }

        /// <summary>
        /// Clears all tracked subscriptions by this instance and returns them.
        /// </summary>
        /// <returns>IEnumerable&lt;ISubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ISubscription<TSchema>> Clear()
        {
            IEnumerable<ISubscription<TSchema>> subs;
            lock (_syncLock)
            {
                subs = new List<ISubscription<TSchema>>(_subsById.Values);
                _subsById.Clear();
                _subsByRoute.Clear();
            }

            return subs;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ISubscription<TSchema>> GetEnumerator()
        {
            var list = new List<ISubscription<TSchema>>();
            lock (_syncLock)
                list.AddRange(_subsById.Values);

            return list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the total count of registered subscriptions.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _subsById.Count;
    }
}