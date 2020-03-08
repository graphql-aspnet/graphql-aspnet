// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Connections.Clients
{
    using System.Net.WebSockets;

    /// <summary>
    /// Helpful enumeration conversions for websocket specific stati.
    /// </summary>
    public static class ClientConnectionEnumerationExtensions
    {
        /// <summary>
        /// Converts the web socket close status into its equivilant internal status.
        /// </summary>
        /// <param name="closeStatus">The websocket close status.</param>
        /// <returns>System.Nullable&lt;ClientConnectionCloseStatus&gt;.</returns>
        public static ClientConnectionCloseStatus ToClientConnectionCloseStatus(this WebSocketCloseStatus closeStatus)
        {
            return (ClientConnectionCloseStatus)(int)closeStatus;
        }

        /// <summary>
        /// Converts the internal close status to its websocket specific status.
        /// </summary>
        /// <param name="closeStatus">The internal close status to convert.</param>
        /// <returns>System.Nullable&lt;WebSocketCloseStatus&gt;.</returns>
        public static WebSocketCloseStatus ToWebSocketCloseStatus(this ClientConnectionCloseStatus closeStatus)
        {
            return (WebSocketCloseStatus)(int)closeStatus;
        }

        /// <summary>
        /// Converts the web socket message type into its equivilant internal type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>ClientMessageType.</returns>
        public static ClientMessageType ToClientMessageType(this WebSocketMessageType messageType)
        {
            return (ClientMessageType)(int)messageType;
        }

        /// <summary>
        /// Converts the internal message type to its web socket specific type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>WebSocketMessageType.</returns>
        public static WebSocketMessageType ToWebSocketMessageType(this ClientMessageType messageType)
        {
            return (WebSocketMessageType)(int)messageType;
        }

        /// <summary>
        /// Converts the web socket state into its internal client state.
        /// </summary>
        /// <param name="socketState">State of the socket.</param>
        /// <returns>ClientConnectionState.</returns>
        public static ClientConnectionState ToClientState(this WebSocketState socketState)
        {
            return (ClientConnectionState)(int)socketState;
        }
    }
}