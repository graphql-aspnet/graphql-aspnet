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
    using System.Collections.Generic;
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
        /// Receives the next segment of data from the connection asynchronously.
        /// </summary>
        /// <param name="buffer">References the application buffer that is the storage location for the received
        ///  data.</param>
        /// <param name="cancelToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>Task&lt;IClientConnectionResult&gt;.</returns>
        Task<IClientConnectionReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancelToken = default);

        /// <summary>
        /// Receives a full, complete and deserializable message from the connection. This method may or may not
        /// produce different results than <see cref="ReceiveAsync"/> depending on the implementation of the underlying
        /// connection.
        /// </summary>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task&lt;System.ValueTuple&lt;WebSocketReceiveResult, IEnumerable&lt;System.Byte&gt;&gt;&gt;.</returns>
        Task<(IClientConnectionReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(CancellationToken cancelToken = default);

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
        /// <param name="subProtocol">The sub protocol the server wishes to tell
        /// the client that it will accept for messaging standards.</param>
        /// <returns>Task.</returns>
        Task OpenAsync(string subProtocol);

        /// <summary>
        /// Gets a description applied by the remote endpoint to describe the why the connection was closed.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake.
        /// </summary>
        /// <value>The final close status if this connection is closed, otherwise null.</value>
        ConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>The current state of this connection.</value>
        ClientConnectionState State { get; }

        /// <summary>
        /// Gets the configured service provider for the client connection.
        /// </summary>
        /// <value>The service provider.</value>
        ///
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the security context governing this connection.
        /// </summary>
        /// <value>The user.</value>
        IUserSecurityContext SecurityContext { get; }

        /// <summary>
        /// Gets a comma delimited list of communications protocol requested by this client connection.
        /// </summary>
        /// <value>The requested set of protocols.</value>
        string RequestedProtocols { get; }

        /// <summary>
        /// Gets the actual communications protocol neogiated by this client connection
        /// when it was opened.
        /// </summary>
        /// <value>The protocol.</value>
        string Protocol { get; }

        /// <summary>
        /// Gets a value indicating whether this connection is permanantly closed.
        /// </summary>
        /// <value><c>true</c> if this connection has been permanantly; otherwise, <c>false</c>.</value>
        bool ClosedForever { get; }

        /// <summary>
        /// Gets a value indicating the size of the receive and send buffer for messages
        /// bound to this connection.
        /// </summary>
        /// <value>The size of the buffer.</value>
        int BufferSize { get; }
    }
}