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
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Security.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A client connection created from a <see cref="WebSocket"/>.
    /// </summary>
    public class WebSocketClientConnection : IClientConnection
    {
        private readonly WebSocket _webSocket;
        private readonly HttpContext _httpContext;
        private readonly IUserSecurityContext _securityContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClientConnection" /> class.
        /// </summary>
        /// <param name="webSocket">The web socket.</param>
        /// <param name="context">The context.</param>
        public WebSocketClientConnection(WebSocket webSocket, HttpContext context)
        {
            _webSocket = Validation.ThrowIfNullOrReturn(webSocket, nameof(WebSocket));
            _httpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
            _securityContext = new HttpUserSecurityContext(_httpContext);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public ClientConnectionCloseStatus? CloseStatus => _webSocket.CloseStatus?.ToClientConnectionCloseStatus();

        /// <inheritdoc />
        public ClientConnectionState State => _webSocket.State.ToClientState();

        /// <inheritdoc />
        public IServiceProvider ServiceProvider => _httpContext.RequestServices;

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext => _securityContext;
    }
}