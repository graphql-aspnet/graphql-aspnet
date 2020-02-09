// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Messaging.Messages;
    using GraphQL.AspNet.Messaging.Messages.Common;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// GraphQL subscription support for Apollo's graphql-over-websockets protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is built for.</typeparam>
    public class ApolloClientProxy<TSchema> : ISubscriptionClientProxy<TSchema>
        where TSchema : class, ISchema
    {
        private SchemaSubscriptionOptions<TSchema> _options;
        private HttpContext _context;
        private WebSocket _socket;
        private ApolloMessageRecievedDelegate _messageDelegate;

        /// <summary>
        /// Occurs just before the underlying websocket is opened. Once completed messages
        /// may be dispatched immedately.
        /// </summary>
        public event EventHandler ConnectionOpening;

        /// <summary>
        /// Raised by a client just after the underlying websocket is shut down. No messages will be sent.
        /// </summary>
        public event EventHandler ConnectionClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="context">The governing http context for this connection.</param>
        /// <param name="socket">The socket connection with the client.</param>
        /// <param name="options">The options used to configure the registration.</param>
        public ApolloClientProxy(HttpContext context, WebSocket socket, SchemaSubscriptionOptions<TSchema> options)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
            _socket = Validation.ThrowIfNullOrReturn(socket, nameof(socket));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
        }

        /// <summary>
        /// Registers a singular, asyncronous delegate to which messages received by this client can
        /// be forwarded. Replace the delegate or pass null to stop message forwarding.
        /// </summary>
        /// <param name="delegateHandler">The delegate handler.</param>
        public void RegisterAsyncronousMessageDelegate(ApolloMessageRecievedDelegate delegateHandler)
        {
            _messageDelegate = delegateHandler;
        }

        /// <summary>
        /// Performs acknowledges the setup of the subscription through the websocket and brokers messages
        /// between the client and the graphql runtime for its lifetime. When this method completes the socket is
        /// closed.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task StartConnection()
        {
            this.ConnectionOpening?.Invoke(this, new EventArgs());

            // register the socket with an "apollo level" keep alive monitor
            // that will send structured keep alive messages down the pipe
            var keepAliveTimer = new ApolloClientConnectionKeepAliveMonitor(this, _options.KeepAliveInterval);
            keepAliveTimer.Start();

            (var result, var bytes) = await _socket.ReceiveFullMessage(_options.MessageBufferSize);

            // message dispatch loop
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = this.DeserializeMessage(bytes);
                    await _messageDelegate?.Invoke(this, message);
                }

                (result, bytes) = await _socket.ReceiveFullMessage(_options.MessageBufferSize);
            }

            // shut down the socket and the apollo-protocol-specific keep alive
            keepAliveTimer.Stop();
            await _socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _socket = null;

            this.ConnectionClosed?.Invoke(this, new EventArgs());

            // unregister any events that may be listening, this subscription is shutting down for good.
            this.DoActionForAllInvokers(this.ConnectionOpening, x => this.ConnectionOpening -= x);
            this.DoActionForAllInvokers(this.ConnectionClosed, x => this.ConnectionClosed -= x);
        }

        /// <summary>
        /// Executes the provided action against all members of the invocation list of the supplied delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate being acted on.</typeparam>
        /// <param name="delegateCollection">The delegate that has an invocation list (can be null).</param>
        /// <param name="action">The action.</param>
        private void DoActionForAllInvokers<TDelegate>(TDelegate delegateCollection, Action<TDelegate> action)
            where TDelegate : Delegate
        {
            if (delegateCollection != null)
            {
                foreach (Delegate d in delegateCollection.GetInvocationList())
                {
                    action(d as TDelegate);
                }
            }
        }

        /// <summary>
        /// Deserializes the text message (represneted as a UTF-8 encoded byte array) into an
        /// appropriate <see cref="ApolloMessage"/>.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>IGraphQLOperationMessage.</returns>
        private ApolloMessage DeserializeMessage(IEnumerable<byte> bytes)
        {
            var text = Encoding.UTF8.GetString(bytes.ToArray());

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            var partialMessage = JsonSerializer.Deserialize<ApolloPartialClientMessage>(text, options);

            return partialMessage.Convert();
        }

        /// <summary>
        /// Serializes, encodes and sends the given message down to the client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task.</returns>
        Task ISubscriptionClientProxy.SendMessage(object message)
        {
            Validation.ThrowIfNull(message, nameof(message));
            Validation.ThrowIfNotCastable<ApolloMessage>(message.GetType(), nameof(message));

            return this.SendMessage(message as ApolloMessage);
        }

        /// <summary>
        /// Serializes, encodes and sends the given message down to the client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task.</returns>
        public Task SendMessage(ApolloMessage message)
        {
            Validation.ThrowIfNull(message, nameof(message));

            var options = new JsonSerializerOptions();
            options.Converters.Add(new ApolloMessageConverter());

            // graphql is defined to communcate in UTF-8
            var bytes = JsonSerializer.SerializeToUtf8Bytes(message, typeof(ApolloMessage), options);
            if (_socket.State == WebSocketState.Open)
            {
                return _socket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    default);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Gets the state of the underlying websocket connection.
        /// </summary>
        /// <value>The state.</value>
        public WebSocketState State => _socket.State;

        /// <summary>
        /// Gets the service provider instance assigned to this client for resolving object requests.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider => _context.RequestServices;

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal" /> representing the user of the client.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User => _context.User;
    }
}