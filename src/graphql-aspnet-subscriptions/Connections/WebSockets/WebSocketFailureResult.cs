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
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A receive result returned by a client when the recieve message fails to complete
    /// successfully or as intended, likely due to an exception.
    /// </summary>
    public class WebSocketFailureResult : IClientConnectionReceiveResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketFailureResult"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public WebSocketFailureResult(WebSocketException exception)
        {
            this.Exception = exception;
            this.CloseStatusDescription = exception.Message;
            this.CloseStatus = ClientConnectionCloseStatus.InternalServerError;
            this.Count = 0;
            this.EndOfMessage = true;
            this.MessageType = ClientMessageType.Close;
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
        public string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the count of bytes retrieved off the connection.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; }

        /// <summary>
        /// Gets a value indicating whether the data received indicates the end of a message.
        /// </summary>
        /// <value><c>true</c> if the bytes retrieved indicate the end of a message; otherwise, <c>false</c>.</value>
        public bool EndOfMessage { get; }

        /// <summary>
        /// Gets a value indicating whether the current message is a UTF-8 message or a binary message.
        /// </summary>
        /// <value>The type of the message that was recieved.</value>
        public ClientMessageType MessageType { get; }

        /// <summary>
        /// Gets the exception thrown that caused this failure result, if any.
        /// </summary>
        /// <value>The exception.</value>
        public WebSocketException Exception { get; }
    }
}