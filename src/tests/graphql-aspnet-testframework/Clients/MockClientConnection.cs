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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Moq;

    /// <summary>
    /// A fake client connection to mock how a websocket would send and recieve data
    /// that is also inspectable for unit tests. This client executes by storing a sequence of messages
    /// in a specific order then, through the connection interface, delivering each in turn to the server listening
    /// to this client.
    /// </summary>
    public class MockClientConnection : IClientConnection
    {
        // a queue of messages send by the client and recieved server side
        private readonly Queue<MockSocketMessage> _incomingMessageQueue;

        // a queue of message sent by the server to the client
        private readonly Queue<MockSocketMessage> _outgoingMessageQueue;

        private readonly bool _autoCloseOnReadCloseMessage;
        private MockSocketMessage _currentMessage;

        private bool _connectionClosed;
        private bool _connectionClosedByServer;
        private bool _wasOpened;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientConnection" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="securityContext">The security context.</param>
        /// <param name="autoCloseOnReadCloseMessage">if set to <c>true</c> when the connection
        /// reads a close message, it will shut it self down.</param>
        /// <param name="requestedProtocol">The requested protocol this client should show.</param>
        /// <param name="actualProtocol">The actual protocol this client would have negoiated.</param>
        public MockClientConnection(
            IServiceProvider serviceProvider = null,
            IUserSecurityContext securityContext = null,
            bool autoCloseOnReadCloseMessage = true,
            string requestedProtocol = null,
            string actualProtocol = null)
        {
            this.ServiceProvider = serviceProvider ?? new Mock<IServiceProvider>().Object;
            this.SecurityContext = securityContext;
            _incomingMessageQueue = new Queue<MockSocketMessage>();

            _outgoingMessageQueue = new Queue<MockSocketMessage>();
            this.State = ClientConnectionState.Open;

            _autoCloseOnReadCloseMessage = autoCloseOnReadCloseMessage;
            this.RequestedProtocols = requestedProtocol;
            this.Protocol = actualProtocol;
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
            lock (_incomingMessageQueue)
            {
                _incomingMessageQueue.Enqueue(message);
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
            lock (_outgoingMessageQueue)
                return _outgoingMessageQueue.Dequeue();
        }

        /// <summary>
        /// Peeks at the next received message.
        /// </summary>
        /// <returns>MockClientMessage.</returns>
        public MockSocketMessage PeekNextReceivedMessage()
        {
            lock (_outgoingMessageQueue)
                return _outgoingMessageQueue.Peek();
        }

        /// <inheritdoc />
        public Task OpenAsync(string subProtocol)
        {
            _wasOpened = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task CloseAsync(ConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            this.ClosedForever = true;

            if (!_wasOpened)
                throw new InvalidOperationException("Cant close a non-open connection");

            // when not already closed (form the client side)
            // then queue a message to indicate the server initiated the close
            if (!_connectionClosed)
                _outgoingMessageQueue.Enqueue(new MockServerCloseMessage(closeStatus, statusDescription));

            _connectionClosed = true;
            _connectionClosedByServer = true;
            this.CloseStatusDescription = statusDescription;
            this.CloseStatus = closeStatus;
            this.State = ClientConnectionState.Closed;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IClientConnectionReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancelToken = default)
        {
            if (!_wasOpened)
                throw new InvalidOperationException("Cant receive a never-opened connection");

            if (_connectionClosed)
                throw new InvalidOperationException("can't recieve on a closed connection");

            while (_currentMessage == null && !_connectionClosed)
            {
                lock (_incomingMessageQueue)
                {
                    if (_incomingMessageQueue.Count > 0)
                        _currentMessage = _incomingMessageQueue.Dequeue();
                }

                if (_currentMessage == null)
                    await Task.Delay(5).ConfigureAwait(false);
            }

            if (_currentMessage == null)
            {
                await Task.Delay(5);
                return new MockClientMessageResult(0, ClientMessageType.Binary, true);
            }

            if (_currentMessage is MockTestActionMessage tam)
            {
                if (tam.Action != null)
                    tam.Action.Invoke();

                _currentMessage = null;
                return new MockClientMessageResult(0, ClientMessageType.Binary, true);
            }

            bool hasRemainingBytes = _currentMessage.ReadNextBytes(buffer, out int bytesRead);

            var result = new MockClientMessageResult(
                bytesRead,
                _currentMessage.MessageType,
                !hasRemainingBytes,
                !hasRemainingBytes ? _currentMessage.CloseStatus : null,
                !hasRemainingBytes ? _currentMessage.CloseStatusDescription : null);

            _connectionClosed = !hasRemainingBytes && result.CloseStatus != null;

            // clear the message from the being read if its complete
            if (!hasRemainingBytes)
                _currentMessage = null;

            return result;
        }

        /// <inheritdoc />
        public async Task<(IClientConnectionReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(CancellationToken cancelToken = default)
        {
            IClientConnectionReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[this.BufferSize];
            do
            {
                response = await this.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            }
            while (!response.EndOfMessage);

            return (response, message);
        }

        /// <inheritdoc />
        public Task SendAsync(byte[] data, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (!_wasOpened)
                throw new InvalidOperationException("Cant send on a never-opened connection");

            var message = new MockSocketMessage(data, messageType, endOfMessage);
            lock (_outgoingMessageQueue)
            {
                _outgoingMessageQueue.Enqueue(message);
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
        public int ResponseMessageCount => _outgoingMessageQueue.Count;

        /// <summary>
        /// Gets the number of simulated messages still on the connection
        /// that have yet to be consumed.
        /// </summary>
        /// <value>The queued message count.</value>
        public int QueuedMessageCount => _incomingMessageQueue.Count;

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets a value indicating whether this connection was closed by way
        /// of the server proxy or itself.
        /// </summary>
        /// <value><c>true</c> if closed by the server; otherwise, <c>false</c>.</value>
        public bool ConnectionClosedByServer => _connectionClosedByServer;

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext { get; }

        /// <inheritdoc />
        public string RequestedProtocols { get; }

        /// <inheritdoc />
        public string Protocol { get; }

        /// <inheritdoc />
        public bool ClosedForever { get; private set; }

        /// <inheritdoc />
        public int BufferSize => 4096;
    }
}