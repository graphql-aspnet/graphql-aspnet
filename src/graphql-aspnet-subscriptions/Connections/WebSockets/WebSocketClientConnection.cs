// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Connections.WebSockets
{
    using System;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A client connection created from a <see cref="WebSocket"/>.
    /// </summary>
    public class WebSocketClientConnection : IClientConnection
    {
        private readonly WebSocket _webSocket;
        private readonly HttpContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClientConnection" /> class.
        /// </summary>
        /// <param name="webSocket">The web socket.</param>
        /// <param name="context">The context.</param>
        public WebSocketClientConnection(WebSocket webSocket, HttpContext context)
        {
            _webSocket = Validation.ThrowIfNullOrReturn(webSocket, nameof(WebSocket));
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        /// <summary>
        /// Closes the connection as an asynchronous operation using the close handshake defined by the underlying implementation.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the connection.</param>
        /// <param name="statusDescription">Specifies a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be
        /// canceled.</param>
        /// <returns>Task.</returns>
        public async Task CloseAsync(ClientConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            try
            {
                await _webSocket.CloseAsync(
                    closeStatus.ToWebSocketCloseStatus(),
                    statusDescription,
                    cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (WebSocketException wse)
            {
                if (wse.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    // ignore
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Receives data from the connection asynchronously.
        /// </summary>
        /// <param name="buffer">References the application buffer that is the storage location for the received
        ///  data.</param>
        /// <param name="cancelToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>Task&lt;IClientConnectionResult&gt;.</returns>
        public async Task<IClientConnectionReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancelToken = default)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(buffer, cancelToken);
                return new WebSocketReceiveResultProxy(result);
            }
            catch (WebSocketException webSocketException)
            {
                return new WebSocketFailureResult(webSocketException);
            }
            catch (AggregateException ae)
            {
                var inner = ae.InnerException;
                if (inner is WebSocketException wse)
                    return new WebSocketFailureResult(wse);
                else
                    return new ClientConnectionFailureResult(inner ?? ae);
            }
            catch (Exception ex)
            {
                return new ClientConnectionFailureResult(ex);
            }
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

        /// <summary>
        /// Gets the configured service provider for the client connection.
        /// </summary>
        /// <value>The service provider.</value>
        ///
        public IServiceProvider ServiceProvider => _context.RequestServices;

        /// <summary>
        /// Gets the authenticated user on the client connection, if any.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User => _context.User;
    }
}