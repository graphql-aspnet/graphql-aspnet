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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    public interface ISubscriptionClientProxy
    {
        /// <summary>
        /// Performs acknowledges the setup of the subscription through the websocket and brokers messages
        /// between the client and the graphql runtime for its lifetime. When this method completes the socket is
        /// closed.
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
        Task ReceiveEvent(GraphFieldPath field, object sourceData, CancellationToken cancelToken = default);

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
        /// Gets the <see cref="ClaimsPrincipal"/> representing the user of the client.
        /// </summary>
        /// <value>The user.</value>
        ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets the state of the underlying connection.
        /// </summary>
        /// <value>The state.</value>
        ClientConnectionState State { get; }

        /// <summary>
        /// Serializes, encodes and sends the given message down to the client.
        /// </summary>
        /// <param name="message">The message to be serialized and sent to the client.</param>
        /// <returns>Task.</returns>
        Task SendMessage(object message);
    }
}