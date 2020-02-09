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
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Execution.Subscriptions.ClientConnections;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A client connection created from a <see cref="WebSocket"/>.
    /// </summary>
    public class WebSocketClientConnection : IClientConnection
    {
        private WebSocket _webSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClientConnection"/> class.
        /// </summary>
        /// <param name="webSocket">The web socket.</param>
        public WebSocketClientConnection(WebSocket webSocket)
        {
            _webSocket = Validation.ThrowIfNullOrReturn(webSocket, nameof(WebSocket));
        }

        /// <summary>
        /// Closes the connection as an asynchronous operation using the close handshake defined by the underlying implementation.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the connection.</param>
        /// <param name="statusDescription">Specifies a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be
        /// canceled.</param>
        /// <returns>Task.</returns>
        public Task CloseAsync(ClientConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            return _webSocket.CloseAsync(
                closeStatus.ToWebSocketCloseStatus(),
                statusDescription,
                cancellationToken);
        }

        /// <summary>
        /// Receives the asynchronous.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;IClientConnectionReceiveResult&gt;.</returns>
        public Task<IClientConnectionReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _webSocket.ReceiveAsync(buffer, cancellationToken)
                .ContinueWith(result => new WebSocketReceiveResultProxy(result.Result) as IClientConnectionReceiveResult);
        }

        /// <summary>
        /// Sends data over the connection asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to be sent over the connection.</param>
        /// <param name="messageType">TIndicates whether the application is sending a binary or text message.</param>
        /// <param name="endOfMessage">Indicates whether the data in "buffer" is the last part of a message.</param>
        /// <param name="cancellationToken">The token that propagates the notification that operations should be canceled.</param>
        /// <returns>Task.</returns>
        public Task SendAsync(ArraySegment<byte> buffer, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return _webSocket.SendAsync(
                buffer,
                messageType.ToWebSocketMessageType(),
                endOfMessage,
                cancellationToken);
        }

        /// <summary>
        /// Getsa description applied by the remote endpoint to describe the why the connection was closed.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        public string CloseStatusDescription => _webSocket.CloseStatusDescription;

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake.
        /// </summary>
        /// <value>The final close status if this connection is closed, otherwise null.</value>
        public ClientConnectionCloseStatus? CloseStatus => _webSocket.CloseStatus?.ToClientConnectionCloseStatus();

        /// <summary>
        /// Gets the current state of the WebSocket connection.
        /// </summary>
        /// <value>The current state of this connection.</value>
        public ClientConnectionState State => _webSocket.State.ToClientState();
    }
}