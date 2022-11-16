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
    public class WebSocketReceiveResultProxy : WebsocketResultBase
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

            this.EndOfMessage = _socketResult.EndOfMessage;
            this.CloseStatusDescription = _socketResult.CloseStatusDescription;
            this.Count = _socketResult.Count;
        }
    }
}