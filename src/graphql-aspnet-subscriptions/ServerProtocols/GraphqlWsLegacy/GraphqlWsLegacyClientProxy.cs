﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Extensions;
    using GraphQL.AspNet.Middleware.SubcriptionExecution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// GraphQL subscription support for GraphqlWsLegacy's graphql-over-websockets protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is built for.</typeparam>
    [DebuggerDisplay("Subscriptions = {_subscriptions.Count}")]
    public class GraphqlWsLegacyClientProxy<TSchema> : ISubscriptionClientProxy<TSchema>
        where TSchema : class, ISchema
    {
        private readonly bool _enableKeepAlive;
        private readonly ClientProxyEventLogger<TSchema> _logger;
        private readonly bool _enableMetrics;
        private readonly SubscriptionServerOptions<TSchema> _options;
        private readonly GraphqlWsLegacyMessageConverterFactory _messageConverter;
        private readonly ClientTrackedMessageIdSet _reservedMessageIds;
        private readonly SubscriptionCollection<TSchema> _subscriptions;
        private IClientConnection _connection;
        private bool _connectionClosedForever;

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
        /// Raised by the client as it begins to shut down. The underlying websocket may
        /// already be closed if the close is client initiated. This event occurs before
        /// any subscriptions are stopped or removed.
        /// </summary>
        public event EventHandler ConnectionClosing;

        /// <summary>
        /// Raised by a client when it starts monitoring a subscription for a given route.
        /// </summary>
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteAdded;

        /// <summary>
        /// Raised by a client when it is no longer monitoring a given subscription route.
        /// </summary>
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="clientConnection">The underlying client connection for GraphqlWsLegacy to manage.</param>
        /// <param name="options">The options used to configure the registration.</param>
        /// <param name="messageConverter">The message converter factory that will generate
        /// json converters for the various <see cref="GraphqlWsLegacyMessage" /> the proxy shuttles to the client.</param>
        /// <param name="protocolName">Name of the protocol this client negotiated as.</param>
        /// <param name="logger">The logger to record client level events to, if any.</param>
        /// <param name="enableMetrics">if set to <c>true</c> any queries this client
        /// executes will have metrics attached.</param>
        public GraphqlWsLegacyClientProxy(
            IClientConnection clientConnection,
            SubscriptionServerOptions<TSchema> options,
            GraphqlWsLegacyMessageConverterFactory messageConverter,
            string protocolName,
            IGraphEventLogger logger = null,
            bool enableMetrics = false)
        {
            this.Protocol = Validation.ThrowIfNullWhiteSpaceOrReturn(protocolName, nameof(protocolName));
            _connection = Validation.ThrowIfNullOrReturn(clientConnection, nameof(clientConnection));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _messageConverter = Validation.ThrowIfNullOrReturn(messageConverter, nameof(messageConverter));

            _reservedMessageIds = new ClientTrackedMessageIdSet();
            _subscriptions = new SubscriptionCollection<TSchema>();
            _enableKeepAlive = options.KeepAliveInterval != TimeSpan.Zero;

            _logger = logger != null ? new ClientProxyEventLogger<TSchema>(this, logger) : null;
            _enableMetrics = enableMetrics;
        }

        /// <inheritdoc />
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
                this.SubscriptionRouteRemoved?.Invoke(this, new SubscriptionFieldEventArgs(field));

            _subscriptions.Clear();
            _reservedMessageIds.Clear();
            this.ConnectionClosed?.Invoke(this, EventArgs.Empty);
            _connectionClosedForever = true;
        }

        /// <inheritdoc />
        public async Task StartConnection()
        {
            if (_connection == null || _connectionClosedForever)
            {
                throw new InvalidOperationException(
                    $"Unable to start this client proxy (id: {this.Id}). It has already " +
                    "been previously closed and cannot be reopened.");
            }

            this.ConnectionOpening?.Invoke(this, EventArgs.Empty);

            // accept the connection and begin lisening
            // for messages related to the protocol known to this specific client type
            await _connection.OpenAsync(GraphqlWsLegacyConstants.PROTOCOL_NAME);

            // register the socket with an "GraphqlWsLegacy level" keep alive monitor
            // that will send structured keep alive messages down the pipe
            GraphqlWsLegacyClientConnectionKeepAliveMonitor<TSchema> keepAliveTimer = null;

            try
            {
                if (_enableKeepAlive)
                {
                    keepAliveTimer = new GraphqlWsLegacyClientConnectionKeepAliveMonitor<TSchema>(this, _options.KeepAliveInterval);
                    keepAliveTimer.Start();
                }

                // message dispatch loop
                IClientConnectionReceiveResult result = null;
                IEnumerable<byte> bytes = null;

                if (_connection.State == ClientConnectionState.Open)
                {
                    do
                    {
                        (result, bytes) = await _connection
                                .ReceiveFullMessage(_options.MessageBufferSize)
                                .ConfigureAwait(false);

                        if (result.MessageType == ClientMessageType.Text)
                        {
                            var message = this.DeserializeMessage(bytes);
                            await this.ProcessReceivedMessage(message)
                                .ConfigureAwait(false);
                        }
                    }
                    while (!result.CloseStatus.HasValue && _connection.State == ClientConnectionState.Open);
                }

                this.ConnectionClosing?.Invoke(this, EventArgs.Empty);

                // shut down the socket and the GraphqlWsLegacy-protocol-specific keep alive
                keepAliveTimer?.Stop();
                keepAliveTimer = null;

                if (this.State == ClientConnectionState.Open)
                {
                    await this.CloseConnection(
                        result.CloseStatus.Value,
                        result.CloseStatusDescription,
                        CancellationToken.None)
                        .ConfigureAwait(false);
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
        /// appropriate <see cref="GraphqlWsLegacyMessage"/>.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>IGraphQLOperationMessage.</returns>
        private GraphqlWsLegacyMessage DeserializeMessage(IEnumerable<byte> bytes)
        {
            var text = Encoding.UTF8.GetString(bytes.ToArray());

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            GraphqlWsLegacyMessage recievedMessage;

            try
            {
                var partialMessage = JsonSerializer.Deserialize<GraphqlWsLegacyClientPartialMessage>(text, options);
                recievedMessage = partialMessage.Convert();
            }
            catch (Exception ex)
            {
                // TODO: Capture deserialization errors as a structured event
                (_logger as IGraphLogger)?.UnhandledExceptionEvent(ex);
                recievedMessage = new GraphqlWsLegacyUnknownMessage(text);
            }

            return recievedMessage;
        }

        /// <inheritdoc />
        public Task SendErrorMessage(IGraphMessage message)
        {
            Validation.ThrowIfNull(message, nameof(message));
            return this.SendMessage(new GraphqlWsLegacyServerErrorMessage(message));
        }

        /// <summary>
        /// Sends a given message down to the connected client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task.</returns>
        public Task SendMessage(GraphqlWsLegacyMessage message)
        {
            Validation.ThrowIfNull(message, nameof(message));

            // create and register the proper serializer for this message
            var options = new JsonSerializerOptions();
            (var converter, var asType) = _messageConverter.CreateConverter<TSchema>(this, message);
            options.Converters.Add(converter);

            // graphql is defined to communcate in UTF-8, serialize the result to that
            var bytes = JsonSerializer.SerializeToUtf8Bytes(message, asType, options);
            if (this.State == ClientConnectionState.Open)
            {
                _logger?.MessageSent(message);
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
        /// Handles the MessageRecieved event of the GraphqlWsLegacyClient control. The client raises this event
        /// whenever a message is recieved and successfully parsed from the under lying websocket.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>TaskMethodBuilder.</returns>
        internal Task ProcessReceivedMessage(GraphqlWsLegacyMessage message)
        {
            if (message == null)
                return Task.CompletedTask;

            _logger?.MessageReceived(message);
            switch (message.Type)
            {
                case GraphqlWsLegacyMessageType.CONNECTION_INIT:
                    return this.AcknowledgeNewConnection();

                case GraphqlWsLegacyMessageType.START:
                    return this.ExecuteStartRequest(message as GraphqlWsLegacyClientStartMessage);

                case GraphqlWsLegacyMessageType.STOP:
                    return this.ExecuteStopRequest(message as GraphqlWsLegacyClientStopMessage);

                case GraphqlWsLegacyMessageType.CONNECTION_TERMINATE:
                    return this.CloseConnection(
                        ClientConnectionCloseStatus.NormalClosure,
                        $"Recieved closure request via message '{GraphqlWsLegacyMessageTypeExtensions.Serialize(GraphqlWsLegacyMessageType.CONNECTION_TERMINATE)}'.");

                default:
                    return this.UnknownMessageRecieved(message);
            }
        }

        /// <summary>
        /// Returns a generic error to the client indicating that the last message recieved was unknown or invalid.
        /// </summary>
        /// <param name="lastMessage">The last message that was received that was unprocessable.</param>
        /// <returns>Task.</returns>
        private Task UnknownMessageRecieved(GraphqlWsLegacyMessage lastMessage)
        {
            var GraphqlWsLegacyError = new GraphqlWsLegacyServerErrorMessage(
                    "The last message recieved was unknown or could not be processed " +
                    "by this server. This graph ql is configured to use GraphqlWsLegacy's GraphQL over websockets " +
                    "message schema.",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: lastMessage,
                    clientProvidedId: lastMessage.Id);

            GraphqlWsLegacyError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_NUMBER,
                "Unknown Message Type");

            GraphqlWsLegacyError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_URL,
                "https://github.com/GraphqlWsLegacygraphql/subscriptions-transport-ws/blob/master/PROTOCOL.md");

            return this.SendMessage(GraphqlWsLegacyError);
        }

        /// <summary>
        /// Attempts to find and remove a subscription with the given client id on the message for the target subscription.
        /// </summary>
        /// <param name="message">The message containing the subscription id to stop.</param>
        /// <returns>Task.</returns>
        private async Task ExecuteStopRequest(GraphqlWsLegacyClientStopMessage message)
        {
            var totalRemaining = _subscriptions.Remove(message.Id, out var subFound);

            if (subFound != null)
            {
                _reservedMessageIds.ReleaseMessageId(subFound.Id);
                if (totalRemaining == 0)
                    this.SubscriptionRouteRemoved?.Invoke(this, new SubscriptionFieldEventArgs(subFound.Field));

                _logger?.SubscriptionStopped(subFound);

                await this
                    .SendMessage(new GraphqlWsLegacyServerCompleteMessage(subFound.Id))
                    .ConfigureAwait(false);
            }
            else
            {
                var errorMessage = new GraphqlWsLegacyServerErrorMessage(
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
        private async Task ExecuteStartRequest(GraphqlWsLegacyClientStartMessage message)
        {
            // ensure the id isnt already in use
            if (!_reservedMessageIds.ReserveMessageId(message.Id))
            {
                await this
                    .SendMessage(new GraphqlWsLegacyServerErrorMessage(
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
            var context = new SubcriptionExecutionContext(
                this,
                request,
                message.Id,
                metricsPackage);

            var result = await runtime
                            .ExecuteRequest(context)
                            .ConfigureAwait(false);

            if (context.IsSubscriptionOperation)
            {
                retainMessageId = await this
                    .RegisterSubscriptionOrRespond(context.Subscription as ISubscription<TSchema>)
                    .ConfigureAwait(false);
            }
            else
            {
                // not a subscription, just send back the generated response and close out the id
                GraphqlWsLegacyMessage responseMessage;

                // report syntax errors as error messages
                // allow others to bubble into a fully reslt (per GraphqlWsLegacy spec)
                if (result.Messages.Count == 1
                    && result.Messages[0].Code == Constants.ErrorCodes.SYNTAX_ERROR)
                {
                    responseMessage = new GraphqlWsLegacyServerErrorMessage(
                          result.Messages[0],
                          message,
                          message.Id);
                }
                else
                {
                    responseMessage = new GraphqlWsLegacyServerDataMessage(message.Id, result);
                }

                await this
                    .SendMessage(responseMessage)
                    .ConfigureAwait(false);
                await this
                    .SendMessage(new GraphqlWsLegacyServerCompleteMessage(message.Id))
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
                    await this.SendMessage(
                        new GraphqlWsLegacyServerErrorMessage(
                            subscription.Messages[0],
                            clientProvidedId: subscription.Id))
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = GraphOperationResult.FromMessages(subscription.Messages, subscription.QueryData);
                    await this
                        .SendMessage(new GraphqlWsLegacyServerDataMessage(subscription.Id, response))
                        .ConfigureAwait(false);
                }

                await this
                    .SendMessage(new GraphqlWsLegacyServerCompleteMessage(subscription.Id))
                    .ConfigureAwait(false);
            }
            else
            {
                var totalTracked = _subscriptions.Add(subscription);
                if (totalTracked == 1)
                    this.SubscriptionRouteAdded?.Invoke(this, new SubscriptionFieldEventArgs(subscription.Field));

                _logger?.SubscriptionCreated(subscription);
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
            await this.SendMessage(new GraphqlWsLegacyServerAckOperationMessage()).ConfigureAwait(false);

            if (_enableKeepAlive)
                await this.SendMessage(new GraphqlWsLegacyKeepAliveOperationMessage()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ReceiveEvent(SchemaItemPath field, object sourceData, CancellationToken cancelToken = default)
        {
            await Task.Yield();

            if (field == null)
                return;

            var subscriptions = _subscriptions.RetreiveByRoute(field);

            _logger?.SubscriptionEventReceived(field, subscriptions);
            if (subscriptions.Count == 0)
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
                    this.SecurityContext,
                    metricsPackage,
                    logger);

                // register the event data as a source input for the target subscription field
                context.DefaultFieldSources.AddSource(subscription.Field, sourceData);
                context.QueryPlan = subscription.QueryPlan;

                tasks.Add(runtime.ExecuteRequest(context, cancelToken)
                    .ContinueWith(
                        task =>
                        {
                            if (task.IsFaulted)
                                return task;

                            // send the message with the resultant data package
                            var message = new GraphqlWsLegacyServerDataMessage(subscription.Id, task.Result);
                            return this.SendMessage(message);
                        },
                        cancelToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public ClientConnectionState State => _connection.State;

        /// <inheritdoc />
        public IServiceProvider ServiceProvider => _connection.ServiceProvider;

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext => _connection.SecurityContext;

        /// <inheritdoc />
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets an enumeration of all the currently tracked subscriptions for this client.
        /// </summary>
        /// <value>The subscriptions.</value>
        public IEnumerable<ISubscription<TSchema>> Subscriptions => _subscriptions;

        /// <inheritdoc />
        public string Protocol { get; }
    }
}