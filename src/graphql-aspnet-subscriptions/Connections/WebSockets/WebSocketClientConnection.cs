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
    using System.Net.Http;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClientConnection" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public WebSocketClientConnection(HttpContext context)
        {
            _httpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
            _securityContext = new HttpUserSecurityContext(_httpContext);

            this.RequestedProtocol = this.DeteremineRequestedProtocol();
        }

        private string DeteremineRequestedProtocol()
        {
            if (_httpContext.Request.Headers.ContainsKey(SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER))
            {
                return _httpContext.Request.Headers[SubscriptionConstants.WebSockets.WEBSOCKET_PROTOCOL_HEADER][0];
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public async Task CloseAsync(ClientConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
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

        /// <inheritdoc />
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
        public Task SendAsync(ArraySegment<byte> buffer, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (this.WebSocket == null)
            {
                throw new InvalidOperationException("Unable to send data packet, this connection is not currently " +
                    "open.");
            }

            return this.WebSocket.SendAsync(
                buffer,
                messageType.ToWebSocketMessageType(),
                endOfMessage,
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task OpenAsync(string protocol)
        {
            if (this.WebSocket != null)
            {
                throw new InvalidOperationException("Unable to open the connection " +
                    "it is already open.");
            }

            this.WebSocket = await _httpContext.WebSockets.AcceptWebSocketAsync(protocol)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Getsa description applied by the remote endpoint to describe the why the connection was closed.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        public string CloseStatusDescription => this.WebSocket.CloseStatusDescription;

        /// <inheritdoc />
        public ClientConnectionCloseStatus? CloseStatus => this.WebSocket.CloseStatus?.ToClientConnectionCloseStatus();

        /// <inheritdoc />
        public ClientConnectionState State => this.WebSocket.State.ToClientState();

        /// <inheritdoc />
        public IServiceProvider ServiceProvider => _httpContext.RequestServices;

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext => _securityContext;

        /// <inheritdoc />
        public string RequestedProtocol { get; }

        /// <summary>
        /// Gets or sets the web socket used by this instance.
        /// </summary>
        /// <value>The web socket.</value>
        protected WebSocket WebSocket { get; set; }
    }
}