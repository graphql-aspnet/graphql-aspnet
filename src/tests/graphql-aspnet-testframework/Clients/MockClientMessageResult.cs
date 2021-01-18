// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.Clients
{
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A fake result mimicing what would be generating when
    /// a client connection recieves data form its underlying implementaiton.
    /// </summary>
    public class MockClientMessageResult : IClientConnectionReceiveResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientMessageResult" /> class.
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="isEndOfMessage">The is end of message.</param>
        /// <param name="closeStatus">The close status.</param>
        /// <param name="closeDescription">The close description.</param>
        public MockClientMessageResult(
                    int byteCount,
                    ClientMessageType messageType,
                    bool isEndOfMessage = false,
                    ClientConnectionCloseStatus? closeStatus = null,
                    string closeDescription = null)
        {
            this.Count = byteCount;
            this.MessageType = messageType;
            this.CloseStatus = closeStatus;
            this.CloseStatusDescription = closeDescription;
            this.EndOfMessage = isEndOfMessage;
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
    }
}