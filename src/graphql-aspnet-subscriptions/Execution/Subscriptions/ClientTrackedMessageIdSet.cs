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
    using GraphQL.AspNet.Common.Generics;

    /// <summary>
    /// A thread safe container for tracking which connected clients have message Ids
    /// in flight on the server.
    /// </summary>
    public class ClientTrackedMessageIdSet
    {
        private ConcurrentHashSet<string> _reservations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTrackedMessageIdSet"/> class.
        /// </summary>
        public ClientTrackedMessageIdSet()
        {
            _reservations = new ConcurrentHashSet<string>();
        }

        /// <summary>
        /// Attempts to resever the given id for the client. A failure to reserve
        /// the id indicates it is already reserved by another request/invocation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the id is reserved successfully, <c>false</c> otherwise.</returns>
        public bool ReserveMessageId(string id)
        {
            if (this.Contains(id))
                return false;

            return _reservations.Add(id);
        }

        /// <summary>
        /// Releases the single message id for the given client. If hte client has no ids registered
        /// no action is taken.
        /// </summary>
        /// <param name="id">The id to release.</param>
        public void ReleaseMessageId(string id)
        {
            _reservations.TryRemove(id);
        }

        /// <summary>
        /// Releases all message ids reserved for the client.
        /// </summary>
        public void Clear()
        {
             _reservations = new ConcurrentHashSet<string>();
        }

        /// <summary>
        /// Determines whether this instance contains a reserved id for the given client.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <returns><c>true</c> if the id is already reserved for the client; otherwise, <c>false</c>.</returns>
        public bool Contains(string id)
        {
            return _reservations.Contains(id);
        }
    }
}