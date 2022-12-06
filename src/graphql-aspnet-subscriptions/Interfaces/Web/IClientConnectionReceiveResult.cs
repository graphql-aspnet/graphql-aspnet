// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Web
{
    using GraphQL.AspNet.Web;

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
        ConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets an optional, human-friendly description that describes why the close handshake has been
        /// initiated by the remote endpoint.
        /// </summary>
        /// <value>The close status description.</value>
        string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the count of bytes retrieved off the connection.
        /// </summary>
        /// <value>The count of bytes retrieved from the connection.</value>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether the current message is a text based message
        /// or a binary message.
        /// </summary>
        /// <value>The type of the message that was recieved.</value>
        ClientMessageType MessageType { get; }
    }
}