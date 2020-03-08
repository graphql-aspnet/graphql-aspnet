// *************************************************************
// project:  graphql-aspnet
// --
// repo: https:///github.com/graphql-aspnet
// docs: https:///graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using GraphQL.AspNet.Connections.Clients;

    /// <summary>
    /// An instance of this class represents the result of performing a single ReceiveAsync
    /// operation on a <see cref="IClientConnection"/>.
    /// </summary>
    public interface IClientConnectionReceiveResult
    {
        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake, if it was closed. Null otherwise.
        /// </summary>
        /// <value>The close status that was provided, if any.</value>
        ClientConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets an optional description that describes why the close handshake has been
        /// initiated by the remote endpoint.
        /// </summary>
        /// <value>The close status description.</value>
        string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the count of bytes retrieved off the connection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether the data received indicates the end of a message.
        /// </summary>
        /// <value><c>true</c> if the bytes retrieved indicate the end of a message; otherwise, <c>false</c>.</value>
        bool EndOfMessage { get; }

        /// <summary>
        /// Gets a value indicating whether the current message is a UTF-8 message or a binary message.
        /// </summary>
        /// <value>The type of the message that was recieved.</value>
        ClientMessageType MessageType { get; }
    }
}