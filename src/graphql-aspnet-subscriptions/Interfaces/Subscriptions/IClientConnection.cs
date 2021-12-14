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
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// An decorator interface exposing the needed communication end points of some underlying
    /// connection (usually a websocket). This interface facliates testing persistant connections in unit tests.
    /// </summary>
    public interface IClientConnection
    {
        /// <summary>
        /// Receives data from the connection asynchronously.
        /// </summary>
        /// <param name="buffer">References the application buffer that is the storage location for the received
        ///  data.</param>
        /// <param name="cancelToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>Task&lt;IClientConnectionResult&gt;.</returns>
        Task<IClientConnectionReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancelToken = default);

        /// <summary>
        /// Closes the connection as an asynchronous operation using the close handshake defined by the underlying implementation.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the connection.</param>
        /// <param name="statusDescription">Specifies a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be
        /// canceled.</param>
        /// <returns>Task.</returns>
        Task CloseAsync(ClientConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// Sends data over the connection asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to be sent over the connection.</param>
        /// <param name="messageType">TIndicates whether the application is sending a binary or text message.</param>
        /// <param name="endOfMessage">Indicates whether the data in "buffer" is the last part of a message.</param>
        /// <param name="cancellationToken">The token that propagates the notification that operations should be canceled.</param>
        /// <returns>Task.</returns>
        Task SendAsync(ArraySegment<byte> buffer, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Getsa description applied by the remote endpoint to describe the why the connection was closed.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake.
        /// </summary>
        /// <value>The final close status if this connection is closed, otherwise null.</value>
        ClientConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>The current state of this connection.</value>
        ClientConnectionState State { get; }

        /// <summary>
        /// Gets the configured service provider for the client connection.
        /// </summary>
        /// <value>The service provider.</value>
        ///
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the security context governing this connection.
        /// </summary>
        /// <value>The user.</value>
        IUserSecurityContext SecurityContext { get; }
    }
}