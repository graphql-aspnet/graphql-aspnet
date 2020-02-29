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
    using System.Collections.Concurrent;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A thread safe container for tracking which connected clients have message Ids
    /// in flight on the server.
    /// </summary>
    public class ClientTrackedMessageIdSet
    {
        private ConcurrentDictionary<ISubscriptionClientProxy, ConcurrentHashSet<string>> _reservations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTrackedMessageIdSet"/> class.
        /// </summary>
        public ClientTrackedMessageIdSet()
        {
            _reservations = new ConcurrentDictionary<ISubscriptionClientProxy, ConcurrentHashSet<string>>();
        }

        /// <summary>
        /// Attempts to resever the given id for the client. A failure to reserve
        /// the id indicates it is already reserved by another request/invocation.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool ReserveMessageId(ISubscriptionClientProxy client, string id)
        {
            ConcurrentHashSet<string> idSet = _reservations.GetOrAdd(
                client,
                (_) => new ConcurrentHashSet<string>());

            if (idSet.Contains(id))
                return false;

            return idSet.Add(id);
        }

        /// <summary>
        /// Releases the single message id for the given client. If hte client has no ids registered
        /// no action is taken.
        /// </summary>
        /// <param name="client">The client to release the id for.</param>
        /// <param name="id">The id to release.</param>
        public void ReleaseMessageId(ISubscriptionClientProxy client, string id)
        {
            if (_reservations.TryGetValue(client, out var ids))
            {
                ids.TryRemove(id);
            }
        }

        /// <summary>
        /// Releases all message ids reserved for the given client and drops the client from
        /// this instance. If the client has no ids registered no action is taken.
        /// </summary>
        /// <param name="client">The client to release.</param>
        public void ReleaseClient(ISubscriptionClientProxy client)
        {
             if (!_reservations.ContainsKey(client))
                return;

             _reservations.TryRemove(client, out var _);
        }

        /// <summary>
        /// Determines whether this instance contains a reserved id for the given client.
        /// </summary>
        /// <param name="client">The client to check.</param>
        /// <param name="id">The identifier to search for.</param>
        /// <returns><c>true</c> if the id is already reserved for the client; otherwise, <c>false</c>.</returns>
        public bool Contains(ISubscriptionClientProxy client, string id)
        {
            if (!_reservations.TryGetValue(client, out var idSet))
                return false;

            return idSet.Contains(id);
        }
    }
}