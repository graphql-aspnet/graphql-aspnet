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
    using System.Net.WebSockets;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A connection receive result that can wrap a raw result retrieved from a websocket.
    /// </summary>
    public class WebSocketReceiveResultProxy : IClientConnectionReceiveResult
    {
        private readonly WebSocketReceiveResult _socketResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketReceiveResultProxy"/> class.
        /// </summary>
        /// <param name="socketResult">The socket result.</param>
        public WebSocketReceiveResultProxy(WebSocketReceiveResult socketResult)
        {
            _socketResult = Validation.ThrowIfNullOrReturn(socketResult, nameof(socketResult));

            this.MessageType = _socketResult.MessageType.ToClientMessageType();
            this.CloseStatus = _socketResult.CloseStatus?.ToClientConnectionCloseStatus();
        }

        /// <inheritdoc />
        public ConnectionCloseStatus? CloseStatus { get; }

        /// <inheritdoc />
        public string CloseStatusDescription => _socketResult.CloseStatusDescription;

        /// <inheritdoc />
        public int Count => _socketResult.Count;

        /// <inheritdoc />
        public bool EndOfMessage => _socketResult.EndOfMessage;

        /// <inheritdoc />
        public ClientMessageType MessageType { get; }
    }
}