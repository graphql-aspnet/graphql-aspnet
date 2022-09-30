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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    public interface ISubscriptionClientProxy : IDisposable
    {
        /// <summary>
        /// Occurs just before the underlying connection is opened.
        /// </summary>
        public event EventHandler ConnectionOpening;

        /// <summary>
        /// Occurs just after the underlying client connection is shut down.This event occurs AFTER
        /// all subscriptions are stopped and removed from the server.
        /// </summary>
        public event EventHandler ConnectionClosed;

        /// <summary>
        /// Occurs as the underlying connection begins to shut down and the client proxy
        /// begins its shutdown sequence. The underlying connection may
        /// already be closed if the operation was client initiated. This event occurs BEFORE
        /// any subscriptions are stopped or removed from the server.
        /// </summary>
        public event EventHandler ConnectionClosing;

        /// <summary>
        /// occurs when the client starts listening for subscriptions against a specific
        /// subscription field in the object graph.
        /// </summary>
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteAdded;

        /// <summary>
        /// Occurs when the client has dropped all subscriptions against a
        /// specific field and is no longer monitoring said field.
        /// </summary>
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteRemoved;

        /// <summary>
        /// Performs the initial setup of the client proxy and begins brokering messages
        /// between the client and the graphql runtime for its lifetime. When this method
        /// completes, the underlying connection is considered permanantly disconnected.
        /// </summary>
        /// <param name="keepAliveInterval">When provided, defines the interval
        /// on which this proxy should issue its keep alive sequence with the connected client.</param>
        /// <param name="initializationTimeout">When provided, defines the amount of
        /// time this proxy should wait for its connected client to transmit the protocol
        /// defined initialization sequence. This value may be ignored by client proxy's whose
        /// messaging protocol does not support initialization time constraints.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task StartConnection(TimeSpan? keepAliveInterval = null, TimeSpan? initializationTimeout = null, CancellationToken cancelToken = default);

        /// <summary>
        /// Instructs the client proxy to immediately close its connection (a server initiated close),
        /// no additional messages should be sent through this instance once closed.
        /// </summary>
        /// <param name="reason">The status reason why the connection is being closed. This may be
        /// sent to the client depending on implementation.</param>
        /// <param name="message">A human readable description as to why the connection was closed by
        /// the server.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task CloseConnection(ConnectionCloseStatus reason, string message = null, CancellationToken cancelToken = default);

        /// <summary>
        /// Called by the server when a new event should be processed by this client instance.
        /// If this is an event the client subscribes to, it should process the data
        /// appropriately and send down any results to its underlying connection as necessary.
        /// </summary>
        /// <param name="field">The unique field corrisponding to the event that was raised
        /// by the publisher.</param>
        /// <param name="sourceData">The source data sent from the publisher when the event was raised.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <remarks>
        /// The data provided is the minimum amount of data to properly represent a subscription:<br/>
        /// 1. The top-level field that the matches the published event that was raised during a mutation<br/>
        /// 2. The object supplied to the publish event when it was raised.
        /// <br/>
        /// It is the responsibility of the client proxy to know its own subscriptions and execute
        /// queries runtime to generate an appropriate graph result and send it to the client.
        /// </remarks>
        /// <returns>Task.</returns>
        Task ReceiveEvent(SchemaItemPath field, object sourceData, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the globally unique id assigned to this client instance. This id is reported
        /// in logging entries.
        /// </summary>
        /// <value>The client's unique id.</value>
        string Id { get; }

        /// <summary>
        /// Gets the security context governing requests made by this client proxy.
        /// </summary>
        /// <value>The security context.</value>
        IUserSecurityContext SecurityContext { get; }

        /// <summary>
        /// Gets the messaing protocol supported by this client.
        /// </summary>
        /// <value>The client's chosen messaging protocol.</value>
        string Protocol { get; }
    }
}