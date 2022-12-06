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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Web;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    public interface ISubscriptionClientProxy : IDisposable
    {
        /// <summary>
        /// Gets the globally unique id assigned to this instance.
        /// </summary>
        /// <value>The instance's unique id.</value>
        SubscriptionClientId Id { get; }

        /// <summary>
        /// Called by an outside source, typically an <see cref="ISubscriptionEventRouter" />,
        /// when an event was raised that this receiver requested.
        /// </summary>
        /// <param name="eventData">The data package representing a raised subscription
        /// event.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        ValueTask ReceiveEvent(SubscriptionEvent eventData, CancellationToken cancelToken = default);

        /// <summary>
        /// Instructs the proxy to perform the initial setup of the client proxy and
        /// begins brokering data between the client and the graphql runtime for its lifetime.
        /// </summary>
        /// <remarks>
        /// When this method completes, the underlying connection is considered permanantly closed
        /// and disconnected.
        /// </remarks>
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
        /// the server. This message is passed directly to the underlying connection.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task CloseConnection(ConnectionCloseStatus reason, string message = null, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the messaging protocol supported by this client. This value is
        /// reported in some log entries. May be <c>null</c> if the subscription proxy does not
        /// advertise or implement a specific protocol.
        /// </summary>
        /// <value>The client's current messaging protocol, if any.</value>
        string Protocol { get; }
    }
}