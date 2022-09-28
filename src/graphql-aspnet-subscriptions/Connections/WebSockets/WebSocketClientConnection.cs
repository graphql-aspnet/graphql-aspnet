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
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Security.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A client connection created from a <see cref="WebSocket"/>.
    /// </summary>
    public class WebSocketClientConnection : IClientConnection
    {
        private readonly HttpContext _httpContext;
        private readonly IUserSecurityContext _securityContext;
        private readonly WebSocketManager _socketManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClientConnection" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="socketManager">The socket manager from which to
        /// accept and process sockets. Defaults to <paramref name="context" /> socket manager
        /// when not supplied.</param>
        /// <param name="bufferSize">Size of the read buffer when reading message data
        /// off the socket (Default: 4k).</param>
        public WebSocketClientConnection(
            HttpContext context,
            WebSocketManager socketManager = null,
            int bufferSize = 4 * 1024)
        {
            _httpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
            _securityContext = new HttpUserSecurityContext(_httpContext);
            _socketManager = socketManager ?? _httpContext.WebSockets;

            this.BufferSize = bufferSize;
            this.RequestedProtocols = this.DeteremineRequestedProtocol();
        }

        private string DeteremineRequestedProtocol()
        {
            if (_httpContext.Request.Headers.ContainsKey(SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER))
            {
                var protocolHeaders = _httpContext.Request.Headers[SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER];
                return string.Join(",", protocolHeaders);
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public async Task CloseAsync(ConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
        {
            this.ClosedForever = true;

            if (this.WebSocket == null)
                return;

            try
            {
                await this.WebSocket.CloseAsync(
                    closeStatus.ToWebSocketCloseStatus(),
                    statusDescription,
                    cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (WebSocketException wse)
            {
                // swallow a premature closure...its what we were aiming for anyways
                if (wse.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                    throw;
            }
        }

        /// <summary>
        /// Receive the next segment of a message as an asynchronous operation.
        /// </summary>
        /// <param name="buffer">The buffer to write the segment to.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>An IClientConnectionReceiveResult representing the results of the read operation.</returns>
        public async Task<IClientConnectionReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancelToken = default)
        {
            if (this.WebSocket == null)
            {
                throw new InvalidOperationException("Unable to receive a data packet, " +
                    "this connection is not currently open.");
            }

            try
            {
                var result = await this.WebSocket.ReceiveAsync(buffer, cancelToken);
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

        /// <inheritdoc />
        public async Task<(IClientConnectionReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(CancellationToken cancelToken = default)
        {
            IClientConnectionReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[this.BufferSize];
            do
            {
                response = await this.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            }
            while (!response.EndOfMessage);

            return (response, message);
        }

        /// <inheritdoc />
        public Task SendAsync(byte[] data, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken = default)
        {
            if (this.WebSocket == null)
            {
                throw new InvalidOperationException("Unable to send data packet, this connection is not currently " +
                    "open.");
            }

            return this.WebSocket.SendAsync(
                new ArraySegment<byte>(data, 0, data.Length),
                messageType.ToWebSocketMessageType(),
                endOfMessage,
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task OpenAsync(string protocol, CancellationToken cancelToken = default)
        {
            if (this.WebSocket != null)
            {
                throw new InvalidOperationException(
                    "Unable to open the connection it is already open.");
            }

            this.WebSocket = await _socketManager.AcceptWebSocketAsync(protocol)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Getsa description applied by the remote endpoint to describe the why the connection was closed.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        public string CloseStatusDescription => this.WebSocket?.CloseStatusDescription;

        /// <inheritdoc />
        public ConnectionCloseStatus? CloseStatus => this.WebSocket?.CloseStatus?.ToClientConnectionCloseStatus();

        /// <inheritdoc />
        public ClientConnectionState State => this.WebSocket?.State.ToClientState() ?? ClientConnectionState.None;

        /// <inheritdoc />
        public IServiceProvider ServiceProvider => _httpContext.RequestServices;

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext => _securityContext;

        /// <inheritdoc />
        public string RequestedProtocols { get; }

        /// <inheritdoc />
        public string Protocol => this.WebSocket?.SubProtocol;

        /// <summary>
        /// Gets or sets the web socket used by this instance.
        /// </summary>
        /// <value>The web socket.</value>
        protected WebSocket WebSocket { get; set; }

        /// <inheritdoc />
        public bool ClosedForever { get; private set; }

        /// <inheritdoc />
        public int BufferSize { get; }
    }
}