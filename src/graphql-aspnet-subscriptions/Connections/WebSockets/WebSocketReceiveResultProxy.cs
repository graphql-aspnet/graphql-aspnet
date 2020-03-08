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

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake, if it was closed. Null otherwise.
        /// </summary>
        /// <value>The close status that was provided, if any.</value>
        public ClientConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets an optional description that describes why the close handshake has been
        /// initiated by the remote endpoint.
        /// </summary>
        /// <value>The close status description.</value>
        public string CloseStatusDescription => _socketResult.CloseStatusDescription;

        /// <summary>
        /// Gets the count of bytes retrieved off the connection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _socketResult.Count;

        /// <summary>
        /// Gets a value indicating whether the data received indicates the end of a message.
        /// </summary>
        /// <value><c>true</c> if the bytes retrieved indicate the end of a message; otherwise, <c>false</c>.</value>
        public bool EndOfMessage => _socketResult.EndOfMessage;

        /// <summary>
        /// Gets a value indicating whether the current message is a UTF-8 message or a binary message.
        /// </summary>
        /// <value>The type of the message that was recieved.</value>
        public ClientMessageType MessageType { get; }
    }
}