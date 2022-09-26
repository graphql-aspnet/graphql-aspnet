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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    public interface ISubscriptionClientProxy
    {
        /// <summary>
        /// Occurs just before the underlying websocket is opened. Once completed messages
        /// may be dispatched immedately.
        /// </summary>
        public event EventHandler ConnectionOpening;

        /// <summary>
        /// Raised by a client just after the underlying websocket is shut down. No further messages will be sent.
        /// </summary>
        public event EventHandler ConnectionClosed;

        /// <summary>
        /// Raised by the client as it begins to shut down. The underlying websocket may
        /// already be closed if the close is client initiated. This event occurs before
        /// any subscriptions are stopped or removed.
        /// </summary>
        public event EventHandler ConnectionClosing;

        /// <summary>
        /// Raised by a client when it starts monitoring a subscription for a given route.
        /// </summary>
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteAdded;

        /// <summary>
        /// Raised by a client when it is no longer monitoring a given subscription route.
        /// </summary>
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteRemoved;

        /// <summary>
        /// Performs the initial setup of the client proxy and begins brokering messages
        /// between the client and the graphql runtime for its lifetime. When this method
        /// completes the connected client is considered permanantly disconnected.
        /// </summary>
        /// <returns>Task.</returns>
        Task StartConnection();

        /// <summary>
        /// Instructs the client to process the new event. If this is an event the client subscribes
        /// to it should process the data appropriately and send down any data to its underlying connection
        /// as necessary.
        /// </summary>
        /// <param name="field">The unique field corrisponding to the event that was raised
        /// by the publisher.</param>
        /// <param name="sourceData">The source data sent from the publisher when the event was raised.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>Task.</returns>
        Task ReceiveEvent(SchemaItemPath field, object sourceData, CancellationToken cancelToken = default);

        /// <summary>
        /// Instructs the client proxy to close its connection from the server side, no additional messages will be sent to it.
        /// </summary>
        /// <param name="reason">The status reason why the connection is being closed. This may be
        /// sent to the client depending on implementation.</param>
        /// <param name="message">A human readonable description as to why the connection was closed by
        /// the server.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>Task.</returns>
        Task CloseConnection(
            ClientConnectionCloseStatus reason,
            string message = null,
            CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the unique id assigned to this client instance.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }

        /// <summary>
        /// Gets the service provider instance assigned to this client for resolving object requests.
        /// </summary>
        /// <value>The service provider.</value>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the Security Context governing this client connection.
        /// </summary>
        /// <value>The user.</value>
        IUserSecurityContext SecurityContext { get; }

        /// <summary>
        /// Gets the state of the underlying connection.
        /// </summary>
        /// <value>The state.</value>
        ClientConnectionState State { get; }

        /// <summary>
        /// Sends the given text as an "error level" message appropriate
        /// for this client's given protocol.
        /// </summary>
        /// <param name="graphMessage">The graph message.</param>
        /// <returns>Task.</returns>
        Task SendErrorMessage(IGraphMessage graphMessage);

        /// <summary>
        /// Serializes, encodes and sends the given message down to the client.
        /// </summary>
        /// <param name="message">The message to be serialized and sent to the client.</param>
        /// <returns>Task.</returns>
        Task SendMessage(object message);

        /// <summary>
        /// Gets the messaing protocol supported by this client.
        /// </summary>
        /// <value>The client's chosen messaging protocol.</value>
        string Protocol { get; }
    }
}