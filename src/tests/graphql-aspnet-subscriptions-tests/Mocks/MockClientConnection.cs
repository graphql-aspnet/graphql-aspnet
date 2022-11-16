// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Web;
    using Microsoft.AspNetCore.Http.Extensions;
    using Moq;

    /// <summary>
    /// A fake client connection to mock how an <see cref="IClientConnection"/> would send and recieve
    /// data that is also inspectable for unit tests. This client executes by queueing a sequence of messages
    /// in a specific order then, through the connection interface, delivering each in turn to the client proxy
    /// listening to this connection.
    /// </summary>
    public class MockClientConnection : IClientConnection
    {
        // a queue of messages send by the client and recieved server side
        private readonly Queue<MockSocketMessage> _clientSentToServerQueue;

        // a queue of message sent by the server to the client
        private readonly Queue<MockSocketMessage> _serverSentToClientQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientConnection" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="securityContext">The security context.</param>
        /// <param name="broadcastState">The state this connection will appear to be in.</param>
        /// <param name="requestedProtocol">The requested protocol this client should show.</param>
        public MockClientConnection(
            IServiceProvider serviceProvider = null,
            IUserSecurityContext securityContext = null,
            ClientConnectionState broadcastState = ClientConnectionState.Connecting,
            string requestedProtocol = null)
        {
            this.ServiceProvider = serviceProvider ?? new Mock<IServiceProvider>().Object;
            this.SecurityContext = securityContext;
            this.State = broadcastState;

            this.RequestedProtocols = requestedProtocol;

            _clientSentToServerQueue = new Queue<MockSocketMessage>();
            _serverSentToClientQueue = new Queue<MockSocketMessage>();
            this.ClosedBy = MockClientConnectionClosedByStatus.NotClosed;
        }

        /// <summary>
        /// Queues a fake "connection closed" message to tell this mock connection to "close itself" mimicing
        /// a websocket disconnecting.
        /// </summary>
        public void QueueConnectionClosedByClient()
        {
            this.QueueClientMessage(new MockClientRemoteCloseMessage());
        }

        /// <summary>
        /// Queues a fake "connection closed" message to tell this mock connection to "close itself" mimicing
        /// a websocket disconnecting.
        /// </summary>
        /// <param name="action">The action to execute when this message is encountered.</param>
        public void QueueAction(Action action)
        {
            this.QueueClientMessage(new MockTestActionMessage(action));
        }

        /// <summary>
        /// Simulates a client message being sent to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        public void QueueClientMessage(MockSocketMessage message)
        {
            Validation.ThrowIfNull(message, nameof(message));
            lock (_clientSentToServerQueue)
            {
                _clientSentToServerQueue.Enqueue(message);
            }
        }

        /// <summary>
        /// Simulates a client message being sent to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        public void QueueClientMessage(object message)
        {
            Validation.ThrowIfNull(message, nameof(message));
            this.QueueClientMessage(new MockSocketMessage(message));
        }

        /// <summary>
        /// Dequeues the next received message and returns it.
        /// </summary>
        /// <returns>MockClientMessage.</returns>
        public MockSocketMessage DequeueNextReceivedMessage()
        {
            lock (_serverSentToClientQueue)
                return _serverSentToClientQueue.Dequeue();
        }

        /// <summary>
        /// Peeks at the next received message.
        /// </summary>
        /// <returns>MockClientMessage.</returns>
        public MockSocketMessage PeekNextReceivedMessage()
        {
            lock (_serverSentToClientQueue)
                return _serverSentToClientQueue.Peek();
        }

        /// <inheritdoc />
        public Task OpenAsync(string subProtocol, CancellationToken cancelToken = default)
        {
            switch (this.State)
            {
                case ClientConnectionState.Connecting:
                    this.State = ClientConnectionState.Open;
                    this.Protocol = subProtocol;
                    break;

                case ClientConnectionState.Open:
                    throw new InvalidOperationException("Can't open an already open connection.");

                default:
                    throw new InvalidOperationException("Can't open a closed or closing connection.");
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task CloseAsync(ConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            // if already closed, don't indicate as server side closed
            if (this.ClosedForever)
                return Task.CompletedTask;

            if (this.State != ClientConnectionState.Open)
                throw new InvalidOperationException("Cant close a non-open connection");

            this.ClosedForever = true;
            this.ClosedBy = MockClientConnectionClosedByStatus.ClosedByServer;
            this.CloseStatusDescription = statusDescription;
            this.CloseStatus = closeStatus;
            this.State = ClientConnectionState.Closed;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IClientConnectionReceiveResult> ReceiveFullMessage(Stream stream, CancellationToken cancelToken = default)
        {
            IClientConnectionReceiveResult response;
            object currentMessage = null;

            if (this.State != ClientConnectionState.Open)
                throw new InvalidOperationException("Cant receive a non-opened connection");

            if (this.ClosedForever)
                throw new InvalidOperationException("Can't recieve on a closed connection");

            while (!this.ClosedForever && currentMessage == null)
            {
                lock (_clientSentToServerQueue)
                {
                    if (_clientSentToServerQueue.Count > 0)
                        currentMessage = _clientSentToServerQueue.Dequeue();
                }

                // no message? wait for one
                if (currentMessage == null)
                    await Task.Delay(5).ConfigureAwait(false);
            }

            if (currentMessage == null)
            {
                await Task.Delay(5);
                response = new MockClientMessageResult(0, ClientMessageType.Ignore, true);
                return response;
            }

            switch (currentMessage)
            {
                case MockTestActionMessage tam:
                    if (tam.Action != null)
                        tam.Action.Invoke();

                    response = new MockClientMessageResult(0, ClientMessageType.Ignore, true);
                    break;

                case MockClientRemoteCloseMessage rcm:
                    this.ClosedForever = true;
                    this.CloseStatus = rcm.CloseStatus;
                    this.CloseStatusDescription = rcm.CloseStatusDescription;
                    this.ClosedBy = MockClientConnectionClosedByStatus.ClosedByClient;

                    response = new MockClientMessageResult(
                        0,
                        ClientMessageType.Close,
                        isEndOfMessage: true,
                        closeStatus: rcm.CloseStatus,
                        closeDescription: rcm.CloseStatusDescription);
                    break;

                case MockSocketMessage msm:
                    bool hasRemainingBytes;
                    var buffer = new byte[this.BufferSize];
                    do
                    {
                        hasRemainingBytes = msm.ReadNextBytes(buffer, out int bytesRead);
                        stream.Write(buffer, 0, bytesRead);
                    }
                    while (hasRemainingBytes);

                    response = new MockClientMessageResult(
                        (int)stream.Length,
                        msm.MessageType,
                        true,
                        msm.CloseStatus,
                        msm.CloseStatusDescription);
                    break;

                default:
                    throw new InvalidOperationException(
                        "Queued client message is invalid with this mocked connection");
            }

            return response;
        }

        /// <inheritdoc />
        public Task SendAsync(byte[] data, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (this.State != ClientConnectionState.Open)
                throw new InvalidOperationException("Cant send data on a non-opened connection");

            var message = new MockSocketMessage(data, messageType);
            lock (_serverSentToClientQueue)
            {
                _serverSentToClientQueue.Enqueue(message);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public string CloseStatusDescription { get; private set; }

        /// <inheritdoc />
        public ConnectionCloseStatus? CloseStatus { get; private set; }

        /// <inheritdoc />
        public ClientConnectionState State { get; private set; }

        /// <summary>
        /// Gets the number of messages recorded as sent by the server to the client.
        /// </summary>
        /// <value>The response message count.</value>
        public int ResponseMessageCount => _serverSentToClientQueue.Count;

        /// <summary>
        /// Gets the number of simulated messages still on the connection
        /// that have yet to be consumed.
        /// </summary>
        /// <value>The queued message count.</value>
        public int QueuedMessageCount => _clientSentToServerQueue.Count;

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext { get; }

        /// <inheritdoc />
        public string RequestedProtocols { get; }

        /// <inheritdoc />
        public string Protocol { get; private set; }

        /// <inheritdoc />
        public bool ClosedForever { get; private set; }

        /// <inheritdoc />
        public int BufferSize => 4096;

        /// <summary>
        /// Gets a list of all queued, but unprocessed messages, sent down to the client.
        /// </summary>
        /// <value>The client received messages.</value>
        public IEnumerable<MockSocketMessage> ClientReceivedMessages => _serverSentToClientQueue;

        /// <summary>
        /// Gets a value indicating whether this connection was closed by way
        /// of the server proxy or the client.
        /// </summary>
        /// <value><c>true</c> if closed by the server; otherwise, <c>false</c>.</value>
        public MockClientConnectionClosedByStatus ClosedBy { get; private set; }

        /// <inheritdoc />
        public CancellationToken RequestAborted { get; } = default;
    }
}