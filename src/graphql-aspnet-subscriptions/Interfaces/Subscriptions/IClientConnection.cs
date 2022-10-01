// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// An decorator interface exposing the needed communication end points of some underlying
    /// connection (usually a websocket). This interface facliates testing persistant connections in unit tests.
    /// </summary>
    public interface IClientConnection
    {
        /// <summary>
        /// Receives a full, complete, and deserializable message, as a set of bytes, from the connection.
        /// </summary>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.ValueTuple&lt;WebSocketReceiveResult, IEnumerable&lt;System.Byte&gt;&gt;&gt;.</returns>
        Task<(IClientConnectionReceiveResult, byte[])> ReceiveFullMessage(CancellationToken cancelToken = default);

        /// <summary>
        /// Closes the connection as an asynchronous operation using the close handshake defined by the underlying implementation.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the connection.</param>
        /// <param name="statusDescription">Specifies a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be
        /// canceled.</param>
        /// <returns>Task.</returns>
        Task CloseAsync(ConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a block of data over the connection asynchronously.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="messageType">TIndicates whether the application is sending a binary or text message.</param>
        /// <param name="endOfMessage">Indicates whether the data in "buffer" is the last part of a message.</param>
        /// <param name="cancellationToken">The token that propagates the notification that operations should be canceled.</param>
        /// <returns>Task.</returns>
        Task SendAsync(byte[] data, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Instructs this client connection to accept any pending requests and begin listening
        /// for messages.
        /// </summary>
        /// <param name="messageProtocol">The message protocol the server will use
        /// to speak with the client. May not be used by all client connection types. </param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task OpenAsync(string messageProtocol, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets an optional, human-friendly description applied by the remote endpoint to describe the why the connection was closed.
        /// Not all remote end points will supply a reason. This value may be null.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the reason code, supplied by the remote end point, on why this connection was closed.
        /// Implementors should always set a value when a connection is closed.
        /// </summary>
        /// <value>The final close status if this connection is closed, otherwise null.</value>
        ConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets the current state of the connection. Not all client connection implementations
        /// will use all state types.
        /// </summary>
        /// <value>The current state of this connection.</value>
        ClientConnectionState State { get; }

        /// <summary>
        /// Gets the configured service provider assigned to the client connection when
        /// the server first recieved it.
        /// </summary>
        /// <value>The service provider.</value>
        ///
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the security context governing this connection, if any. Can be null.
        /// </summary>
        /// <value>The user.</value>
        IUserSecurityContext SecurityContext { get; }

        /// <summary>
        /// Gets a comma delimited list of communications protocol requested by this client connection.
        /// </summary>
        /// <value>The requested set of protocols.</value>
        string RequestedProtocols { get; }

        /// <summary>
        /// Gets the actual communications protocol neogiated by the server when
        /// it accepted the connection.
        /// </summary>
        /// <value>The protocol.</value>
        string Protocol { get; }

        /// <summary>
        /// Gets a value indicating whether this connection is permanantly closed and cannot be opened.
        /// </summary>
        /// <value><c>true</c> if this connection has been permanantly; otherwise, <c>false</c>.</value>
        bool ClosedForever { get; }

        /// <summary>
        /// Gets a value indicating the size of the receive and send buffer for messages
        /// bound to this connection.
        /// </summary>
        /// <value>The size of this connection's recieving buffer.</value>
        int BufferSize { get; }

        /// <summary>
        /// Gets a cancellation token that will trigger when this connection is
        /// closed or no longer active.
        /// </summary>
        /// <value>A cancellation token indiciating this connection is closed.</value>
        CancellationToken RequestAborted { get; }
    }
}