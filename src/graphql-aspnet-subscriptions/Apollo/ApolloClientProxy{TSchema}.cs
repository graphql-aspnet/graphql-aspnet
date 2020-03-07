﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Apollo.Messages;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Execution.Subscriptions.ClientConnections;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubscriptionQueryExecution;
    using GraphQL.AspNet.Schemas.Structural;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// GraphQL subscription support for Apollo's graphql-over-websockets protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is built for.</typeparam>
    public class ApolloClientProxy<TSchema> : ISubscriptionClientProxy<TSchema>
        where TSchema : class, ISchema
    {
        private readonly bool _enableKeepAlive;
        private readonly bool _enableMetrics;
        private readonly SubscriptionServerOptions<TSchema> _options;
        private readonly ApolloMessageConverterFactory _messageConverter;
        private readonly ClientTrackedMessageIdSet _reservedMessageIds;
        private IClientConnection _connection;
        private ApolloSubscriptionCollection<TSchema> _subscriptions;

        /// <summary>
        /// Occurs just before the underlying websocket is opened. Once completed messages
        /// may be dispatched immedately.
        /// </summary>
        public event EventHandler ConnectionOpening;

        /// <summary>
        /// Raised by a client just after the underlying websocket is shut down. No further messages will be sent.
        /// </summary>
        public event EventHandler ConnectionClosed;

        /// <summary>
        /// Raised by a client when it starts monitoring a subscription for a given route.
        /// </summary>
        public event EventHandler<ApolloSubscriptionFieldEventArgs> SubscriptionRouteAdded;

      /// <summary>
        /// Raised by a client when it is no longer monitoring a given subscription route.
        /// </summary>
        public event EventHandler<ApolloSubscriptionFieldEventArgs> SubscriptionRouteRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="clientConnection">The underlying client connection for apollo to manage.</param>
        /// <param name="options">The options used to configure the registration.</param>
        /// <param name="messageConverter">The message converter factory that will generate
        /// json converters for the various <see cref="ApolloMessage" /> the proxy shuttles to the client.</param>
        /// <param name="enableMetrics">if set to <c>true</c> any queries this client
        /// executes will have metrics attached.</param>
        public ApolloClientProxy(
            IClientConnection clientConnection,
            SubscriptionServerOptions<TSchema> options,
            ApolloMessageConverterFactory messageConverter,
            bool enableMetrics = false)
        {
            _connection = Validation.ThrowIfNullOrReturn(clientConnection, nameof(clientConnection));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _messageConverter = Validation.ThrowIfNullOrReturn(messageConverter, nameof(messageConverter));
            _reservedMessageIds = new ClientTrackedMessageIdSet();
            _subscriptions = new ApolloSubscriptionCollection<TSchema>();
            _enableKeepAlive = options.KeepAliveInterval != TimeSpan.Zero;
            _enableMetrics = enableMetrics;
        }

        /// <summary>
        /// Instructs the client proxy to close its connection from the server side, no additional messages will be sent to it.
        /// </summary>
        /// <param name="reason">The status reason why the connection is being closed. This may be
        /// sent to the client depending on implementation.</param>
        /// <param name="message">A human readonable description as to why the connection was closed by
        /// the server.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>Task.</returns>
        public Task CloseConnection(
            ClientConnectionCloseStatus reason,
            string message = null,
            CancellationToken cancelToken = default)
        {
            this.ProcessCloseRequest();
            if (_connection.State == ClientConnectionState.Open)
            {
                return _connection.CloseAsync(reason, message, cancelToken);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Informs any event listeners that this client is shutting down and discontinuing all subscriptions.
        /// </summary>
        private void ProcessCloseRequest()
        {
            // discontinue all client registered subscriptions
            // and acknowledge the terminate request
            var fields = _subscriptions.Select(x => x.Field).Distinct();
            foreach (var field in fields)
                this.SubscriptionRouteRemoved?.Invoke(this, new ApolloSubscriptionFieldEventArgs(field));

            _subscriptions.Clear();
            _reservedMessageIds.Clear();
            this.ConnectionClosed?.Invoke(this, new EventArgs());
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
            ApolloClientConnectionKeepAliveMonitor keepAliveTimer = null;

            try
            {
                if (_enableKeepAlive)
                {
                    keepAliveTimer = new ApolloClientConnectionKeepAliveMonitor(this, _options.KeepAliveInterval);
                    keepAliveTimer.Start();
                }

                (var result, var bytes) = await _connection.ReceiveFullMessage(_options.MessageBufferSize);

                // message dispatch loop
                while (!result.CloseStatus.HasValue && _connection.State == ClientConnectionState.Open)
                {
                    if (result.MessageType == ClientMessageType.Text)
                    {
                        var message = this.DeserializeMessage(bytes);
                        await this.DispatchMessage(message);
                    }

                    (result, bytes) = await _connection.ReceiveFullMessage(_options.MessageBufferSize);
                }

                // shut down the socket and the apollo-protocol-specific keep alive
                keepAliveTimer?.Stop();
                keepAliveTimer = null;

                if (_connection.State == ClientConnectionState.Open)
                {
                    await this.CloseConnection(
                        result.CloseStatus.Value,
                        result.CloseStatusDescription,
                        CancellationToken.None);
                }
                else
                {
                    this.ProcessCloseRequest();
                }

                _connection = null;
            }
            finally
            {
                // ensure keep alive is stopped even when a server exception may be thrown
                // during message receiving
                keepAliveTimer?.Stop();
            }

            // unregister any events that may be listening to this client as its shutting down for good.
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

            ApolloMessage recievedMessage;

            try
            {
                var partialMessage = JsonSerializer.Deserialize<ApolloClientPartialMessage>(text, options);
                recievedMessage = partialMessage.Convert();
            }
            catch
            {
                // TODO: Capture deserialization errors as a structured event
                recievedMessage = new ApolloUnknownMessage(text);
            }

            return recievedMessage;
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

            // create and register the proper message serializer for this message
            var options = new JsonSerializerOptions();
            (var converter, var asType) = _messageConverter.CreateConverter<TSchema>(this, message);
            options.Converters.Add(converter);

            // graphql is defined to communcate in UTF-8, serialize the result to that
            var bytes = JsonSerializer.SerializeToUtf8Bytes(message, asType, options);
            if (_connection.State == ClientConnectionState.Open)
            {
                return _connection.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    ClientMessageType.Text,
                    true,
                    default);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Handles the MessageRecieved event of the ApolloClient control. The client raises this event
        /// whenever a message is recieved and successfully parsed from the under lying websocket.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>TaskMethodBuilder.</returns>
        private Task DispatchMessage(ApolloMessage message)
        {
            if (message == null)
                return Task.CompletedTask;

            switch (message.Type)
            {
                case ApolloMessageType.CONNECTION_INIT:
                    return this.AcknowledgeNewConnection();

                case ApolloMessageType.START:
                    return this.ExecuteStartRequest(message as ApolloClientStartMessage);

                case ApolloMessageType.STOP:
                    return this.ExecuteStopRequest(message as ApolloClientStopMessage);

                case ApolloMessageType.CONNECTION_TERMINATE:
                    return this.CloseConnection(
                        ClientConnectionCloseStatus.NormalClosure,
                        $"Recieved closure request via message '{ApolloMessageTypeExtensions.Serialize(ApolloMessageType.CONNECTION_TERMINATE)}'.");

                default:
                    return this.UnknownMessageRecieved(message);
            }
        }

        /// <summary>
        /// Returns a generic error to the client indicating that the last message recieved was unknown or invalid.
        /// </summary>
        /// <param name="lastMessage">The last message that was received that was unprocessable.</param>
        /// <returns>Task.</returns>
        private Task UnknownMessageRecieved(ApolloMessage lastMessage)
        {
            var apolloError = new ApolloServerErrorMessage(
                    "The last message recieved was unknown or could not be processed " +
                    "by this server. This graph ql is configured to use Apollo's GraphQL over websockets " +
                    "message schema.",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: lastMessage,
                    clientProvidedId: lastMessage.Id);

            apolloError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_NUMBER,
                "Unknown Message Type");

            apolloError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_URL,
                "https://github.com/apollographql/subscriptions-transport-ws/blob/master/PROTOCOL.md");

            return this.SendMessage(apolloError);
        }

        /// <summary>
        /// Attempts to find and remove a subscription with the given client id on the message for the target subscription.
        /// </summary>
        /// <param name="message">The message containing the subscription id to stop.</param>
        /// <returns>Task.</returns>
        private async Task ExecuteStopRequest(ApolloClientStopMessage message)
        {
            var totalRemaining = _subscriptions.Remove(message.Id, out var subFound);

            if (subFound != null)
            {
                _reservedMessageIds.ReleaseMessageId(subFound.Id);
                if (totalRemaining == 0)
                    this.SubscriptionRouteRemoved?.Invoke(this, new ApolloSubscriptionFieldEventArgs(subFound.Field));

                await this
                    .SendMessage(new ApolloServerCompleteMessage(subFound.Id))
                    .ConfigureAwait(false);
            }
            else
            {
                var errorMessage = new ApolloServerErrorMessage(
                    $"No active subscription exists with id '{message.Id}'",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: message,
                    clientProvidedId: message.Id);

                await this
                    .SendMessage(errorMessage)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Parses the message contents to generate a valid client subscription and adds it to the watched
        /// set for this instance.
        /// </summary>
        /// <param name="message">The message with the subscription details.</param>
        private async Task ExecuteStartRequest(ApolloClientStartMessage message)
        {
            // ensure the id isnt already in use
            if (!_reservedMessageIds.ReserveMessageId(message.Id))
            {
                await this
                    .SendMessage(new ApolloServerErrorMessage(
                        $"The message id {message.Id} is already reserved for an outstanding request and cannot " +
                        "be processed against. Allow the in-progress request to complete or stop the associated subscription.",
                        SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID,
                        lastMessage: message,
                        clientProvidedId: message.Id))
                    .ConfigureAwait(false);

                return;
            }

            var retainMessageId = false;
            var runtime = this.ServiceProvider.GetRequiredService(typeof(IGraphQLRuntime<TSchema>)) as IGraphQLRuntime<TSchema>;
            var request = runtime.CreateRequest(message.Payload);
            var metricsPackage = _enableMetrics ? runtime.CreateMetricsPackage() : null;
            var context = new ApolloQueryExecutionContext(
                this,
                request,
                message.Id,
                metricsPackage);

            var result = await runtime
                            .ExecuteRequest(context)
                            .ConfigureAwait(false);

            if (context.IsSubscriptionOperation)
            {
                retainMessageId = await this.RegisterSubscriptionOrRespond(context.Subscription as ISubscription<TSchema>);
            }
            else
            {
                // not a subscription, just send back the generated response and close out the id
                ApolloMessage responseMessage;

                // report syntax errors as error messages
                // allow others to bubble into a fully reslt (per apollo spec)
                if (result.Messages.Count == 1
                    && result.Messages[0].Code == Constants.ErrorCodes.SYNTAX_ERROR)
                {
                    responseMessage = new ApolloServerErrorMessage(
                          result.Messages[0],
                          message,
                          message.Id);
                }
                else
                {
                    responseMessage = new ApolloServerDataMessage(message.Id, result);
                }

                await this
                    .SendMessage(responseMessage)
                    .ConfigureAwait(false);
                await this
                    .SendMessage(new ApolloServerCompleteMessage(message.Id))
                    .ConfigureAwait(false);
            }

            if (!retainMessageId)
                _reservedMessageIds.ReleaseMessageId(message.Id);
        }

        private async Task<bool> RegisterSubscriptionOrRespond(ISubscription<TSchema> subscription)
        {
            var registrationComplete = false;
            if (!subscription.IsValid)
            {
                if (subscription.Messages.Count == 1)
                {
                    await subscription.Client.SendMessage(
                        new ApolloServerErrorMessage(
                            subscription.Messages[0],
                            clientProvidedId: subscription.Id))
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = GraphOperationRequest.FromMessages(subscription.Messages, subscription.QueryData);
                    await subscription.Client
                        .SendMessage(new ApolloServerDataMessage(subscription.Id, response))
                        .ConfigureAwait(false);
                }

                await subscription.Client
                    .SendMessage(new ApolloServerCompleteMessage(subscription.Id))
                    .ConfigureAwait(false);
            }
            else
            {
                var totalTracked = _subscriptions.Add(subscription);
                if (totalTracked == 1)
                    this.SubscriptionRouteAdded?.Invoke(this, new ApolloSubscriptionFieldEventArgs(subscription.Field));

                registrationComplete = true;
            }

            return registrationComplete;
        }

        /// <summary>
        /// Sends the required startup messages down to the connected client to acknowledge the connection/protocol.
        /// </summary>
        private async Task AcknowledgeNewConnection()
        {
            // protocol dictates the messages must be sent in this order
            // await each send before attempting the next one
            await this.SendMessage(new ApolloServerAckOperationMessage()).ConfigureAwait(false);

            if (_enableKeepAlive)
                await this.SendMessage(new ApolloKeepAliveOperationMessage()).ConfigureAwait(false);
        }

        /// <summary>
        /// Instructs the client to process the new event. If this is an event the client subscribes
        /// to it should process the data appropriately and send down any data to its underlying connection
        /// as necessary.
        /// </summary>
        /// <param name="field">The unique field corrisponding to the event that was raised
        /// by the publisher.</param>
        /// <param name="sourceData">The source data sent from the publisher when the event was raised.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ReceiveEvent(GraphFieldPath field, object sourceData, CancellationToken cancelToken = default)
        {
            await Task.Yield();
            var subscriptions = _subscriptions.RetreiveByRoute(field);
            if (!subscriptions.Any())
                return;

            var runtime = this.ServiceProvider.GetRequiredService<IGraphQLRuntime<TSchema>>();
            var schema = this.ServiceProvider.GetRequiredService<TSchema>();

            var tasks = new List<Task>();
            foreach (var subscription in subscriptions)
            {
                IGraphQueryExecutionMetrics metricsPackage = null;
                IGraphEventLogger logger = this.ServiceProvider.GetService<IGraphEventLogger>();

                if (schema.Configuration.ExecutionOptions.EnableMetrics)
                {
                    var factory = this.ServiceProvider.GetRequiredService<IGraphQueryExecutionMetricsFactory<TSchema>>();
                    metricsPackage = factory.CreateMetricsPackage();
                }

                var context = new GraphQueryExecutionContext(
                    runtime.CreateRequest(subscription.QueryData),
                    this.ServiceProvider,
                    this.User,
                    metricsPackage,
                    logger);

                // register the event data as a source input for the target subscription field
                context.DefaultFieldSources.AddSource(subscription.Field, sourceData);
                context.QueryPlan = subscription.QueryPlan;
                context.QueryOperation = subscription.QueryOperation;

                tasks.Add(runtime.ExecuteRequest(context, cancelToken)
                    .ContinueWith(
                        task =>
                        {
                            if (task.IsFaulted)
                                return task;

                            // send the message with the resultant data package
                            var message = new ApolloServerDataMessage(subscription.Id, task.Result);
                            return this.SendMessage(message);
                        },
                        cancelToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the state of the underlying connection.
        /// </summary>
        /// <value>The state.</value>
        public ClientConnectionState State => _connection.State;

        /// <summary>
        /// Gets the service provider instance assigned to this client for resolving object requests.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider => _connection.ServiceProvider;

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal" /> representing the user of the client.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User => _connection.User;

        /// <summary>
        /// Gets the unique id assigned to this client instance.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; } = Guid.NewGuid().ToString();
    }
}