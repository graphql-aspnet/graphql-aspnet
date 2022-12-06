// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web.WebSockets
{
    using System.Net.WebSockets;

    /// <summary>
    /// A receive result returned by a client when the recieve message fails to complete
    /// successfully or as intended, likely due to an exception.
    /// </summary>
    public class WebSocketFailureResult : WebsocketResultBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketFailureResult"/> class.
        /// </summary>
        /// <param name="exception">The exception that was thrown by the connection.</param>
        public WebSocketFailureResult(WebSocketException exception)
        {
            this.Exception = exception;
            this.CloseStatusDescription = exception.Message;
            this.CloseStatus = ConnectionCloseStatus.InternalServerError;
            this.Count = 0;
            this.EndOfMessage = true;
            this.MessageType = ClientMessageType.Close;
        }

        /// <summary>
        /// Gets the exception thrown that caused this failure result, if any.
        /// </summary>
        /// <value>The exception.</value>
        public WebSocketException Exception { get; }
    }
}