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
    using GraphQL.AspNet;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions.ClientConnections;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A fake client connection to mock how a websocket would send and recieve data
    /// that is also inspectable for unit tests. This client executes by storing a sequence of messages
    /// in a specific order then, through the connection interface, delivering each in turn to the server listening
    /// to this client.
    /// </summary>
    public class MockClientConnection : IClientConnection
    {
        // a queue of messages send by the client and recieved server side
        private readonly Queue<MockClientMessage> _incomingMessageQueue;

        // a queue of message sent by the server to the client
        private readonly Queue<MockClientMessage> _outgoingMessageQueue;
        private MockClientMessage _currentMessage;

        private bool _connectionClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientConnection"/> class.
        /// </summary>
        public MockClientConnection()
        {
            _incomingMessageQueue = new Queue<MockClientMessage>();

            _outgoingMessageQueue = new Queue<MockClientMessage>();
            this.State = ClientConnectionState.Open;
        }

        /// <summary>
        /// Queues a fake "connection closed" message to tell this mock connection to "close itself" mimicing
        /// a websocket disconnecting. that will close the underlying "socket" when dequeued.
        /// </summary>
        public void QueueConnectionCloseMessage()
        {
            this.QueueClientMessage(new MockClientRemoteCloseMessage());
        }

        /// <summary>
        /// Simulates a client message being sent to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        public void QueueClientMessage(MockClientMessage message)
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
        public void QueueClientMessage(ApolloMessage message)
        {
            Validation.ThrowIfNull(message, nameof(message));
            this.QueueClientMessage(new MockClientMessage(message));
        }

        /// <summary>
        /// Simulates a client message that starts a new subscription on the server.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="queryText">The query text.</param>
        public void QueueNewSubscription(string id, string queryText)
        {
            this.QueueClientMessage(new ApolloClientStartMessage()
            {
                Id = id,
                Payload = new GraphQueryData()
                {
                    Query = queryText,
                },
            });
        }

        /// <summary>
        /// Dequeues the next received message and returns it.
        /// </summary>
        /// <returns>MockClientMessage.</returns>
        public MockClientMessage DequeueNextReceivedMessage()
        {
            lock (_outgoingMessageQueue)
                return _outgoingMessageQueue.Dequeue();
        }

        /// <summary>
        /// Peeks at the next received message.
        /// </summary>
        /// <returns>MockClientMessage.</returns>
        public MockClientMessage PeekNextReceivedMessage()
        {
            lock (_outgoingMessageQueue)
                return _outgoingMessageQueue.Peek();
        }

        /// <summary>
        /// Closes the connection as an asynchronous operation using the close handshake defined by the underlying implementation.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the connection.</param>
        /// <param name="statusDescription">Specifies a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be
        /// canceled.</param>
        /// <returns>Task.</returns>
        public Task CloseAsync(ClientConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            _connectionClosed = true;
            this.CloseStatusDescription = statusDescription;
            this.CloseStatus = closeStatus;
            this.State = ClientConnectionState.Closed;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Receives data from the connection asynchronously.
        /// </summary>
        /// <param name="buffer">References the application buffer that is the storage location for the received
        ///  data.</param>
        /// <param name="cancelToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>Task&lt;IClientConnectionResult&gt;.</returns>
        public async Task<IClientConnectionReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancelToken = default)
        {
            if (_connectionClosed)
            {
                throw new InvalidOperationException("can't recieve on a closed connection");
            }

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
                throw new InvalidOperationException("No message was recieved, this should be impossible.");

            bool hasRemainingBytes = _currentMessage.ReadNextBytes(buffer, out int bytesRead);

            var result = new MockClientMessageResult(
                bytesRead,
                _currentMessage.MessageType,
                !hasRemainingBytes,
                !hasRemainingBytes ? _currentMessage.CloseStatus : null,
                !hasRemainingBytes ? _currentMessage.CloseStatusDescription : null);

            _connectionClosed = !hasRemainingBytes && result.CloseStatus != null;
            if (_connectionClosed)
            {
                await this
                    .CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancelToken)
                    .ConfigureAwait(false);
            }

            // clear the message from the being read if its complete
            if (!hasRemainingBytes)
                _currentMessage = null;

            return result;
        }

        /// <summary>
        /// Sends data over the connection asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to be sent over the connection.</param>
        /// <param name="messageType">TIndicates whether the application is sending a binary or text message.</param>
        /// <param name="endOfMessage">Indicates whether the data in "buffer" is the last part of a message.</param>
        /// <param name="cancellationToken">The token that propagates the notification that operations should be canceled.</param>
        /// <returns>Task.</returns>
        public Task SendAsync(ArraySegment<byte> buffer, ClientMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            var message = new MockClientMessage(buffer.ToArray(), messageType, endOfMessage);
            lock (_outgoingMessageQueue)
            {
                _outgoingMessageQueue.Enqueue(message);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Getsa description applied by the remote endpoint to describe the why the connection was closed.
        /// </summary>
        /// <value>The description applied when this connection was closed.</value>
        public string CloseStatusDescription { get; private set; }

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake.
        /// </summary>
        /// <value>The final close status if this connection is closed, otherwise null.</value>
        public ClientConnectionCloseStatus? CloseStatus { get; private set; }

        /// <summary>
        /// Gets the current state of the WebSocket connection.
        /// </summary>
        /// <value>The current state of this connection.</value>
        public ClientConnectionState State { get; private set; }

        /// <summary>
        /// Gets the number of messages recorded as sent by the server to the client.
        /// </summary>
        /// <value>The response message count.</value>
        public int ResponseMessageCount => _outgoingMessageQueue.Count;
    }
}