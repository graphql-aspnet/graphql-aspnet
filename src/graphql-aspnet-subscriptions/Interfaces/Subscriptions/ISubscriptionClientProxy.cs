// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    public interface ISubscriptionClientProxy : ISubscriptionEventReceiver, IDisposable
    {
        /// <summary>
        /// Instructs the proxy to perform the initial setup of the client proxy and
        /// begins brokering data between the client and the graphql runtime for its lifetime.
        /// When this method completes, the underlying connection is considered permanantly closed
        /// and disconnected.
        /// </summary>
        /// <param name="keepAliveInterval">When provided, defines the interval
        /// on which this proxy should issue its keep alive sequence with the connected client.
        /// Use of this parameter may not be supported by all proxy types.</param>
        /// <param name="initializationTimeout">When provided, defines the amount of
        /// time this proxy should wait for its connected client to transmit the protocol
        /// defined initialization sequence. Use of this parameter may not be supported by
        /// all proxy types.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task StartConnection(TimeSpan? keepAliveInterval = null, TimeSpan? initializationTimeout = null, CancellationToken cancelToken = default);

        /// <summary>
        /// Instructs the client proxy to immediately close its connection as a "server initiated" close.
        /// No additional messages should be sent through this instance once closed and this proxy
        /// should immediately unregister any active subscriptions.
        /// </summary>
        /// <param name="reason">The status reason indicating why the connection is being closed. This may be
        /// sent to the client depending on implementation.</param>
        /// <param name="message">A human readable description as to why the connection was closed by
        /// the server.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task CloseConnection(ConnectionCloseStatus reason, string message = null, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the messaing protocol supported by this client. This value is
        /// reported in some log entries. May be null if the subscription proxy does not
        /// advertise or implement a specific protocol.
        /// </summary>
        /// <value>The client's current messaging protocol.</value>
        string Protocol { get; }
    }
}