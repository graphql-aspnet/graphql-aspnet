// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscrptions.Tests.CommonHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Castle.DynamicProxy.Generators;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Execution.Subscriptions.ClientConnections;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using NUnit.Framework;

    /// <summary>
    /// A fake client connection to mock how a websocket would send and recieve data
    /// that is also inspectable for unit tests.
    /// </summary>
    public class MockClientConnection : IClientConnection
    {
        // a queue of messages send by the client and recieved server side
        private Queue<MockClientMessage> _incomingMessageQueue;

        // a queue of message sent by the server to the client
        private Queue<MockClientMessage> _outgoingMessageQueue;
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

        public void QueueConnectionClose()
        {
            this.QueueClientMessage(new MockClientRemoteCloseMessage());
        }

        internal T DequeueReturnMessageTo<T>()
            where T : class
        {
            if (_outgoingMessageQueue.Count == 0)
                return null;

            var message = _outgoingMessageQueue.Dequeue();
            var str = Encoding.UTF8.GetString(message.Data);

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;

            return System.Text.Json.JsonSerializer.Deserialize<T>(str, options);
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

        public Task CloseAsync(ClientConnectionCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            _connectionClosed = true;
            this.CloseStatusDescription = statusDescription;
            this.CloseStatus = closeStatus;
            this.State = ClientConnectionState.Closed;
            return Task.CompletedTask;
        }

        public async Task<IClientConnectionReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancelToken = default)
        {
            if (_connectionClosed)
            {
                Assert.Fail("Attempted to recieve on a closed connection");
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
                    await Task.Delay(5);
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
                await this.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancelToken);
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