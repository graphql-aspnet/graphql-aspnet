// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Extensions;
    using GraphQL.AspNet.SubscriptionServer.Protocols.Common;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Converters;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages;
    using GraphQL.AspNet.Web;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// subscription support for the graphql-ws protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is built for.</typeparam>
    [DebuggerDisplay("Subscriptions = {_subscriptions.Count}")]
    internal sealed class GqltwsClientProxy<TSchema> : SubscriptionClientProxyBase<TSchema, GqltwsMessage>
        where TSchema : class, ISchema
    {
        private static readonly JsonSerializerOptions _deserializerOptions;
        private bool _isDisposed;

        /// <summary>
        /// Initializes static members of the <see cref="GqltwsClientProxy{TSchema}"/> class.
        /// </summary>
        static GqltwsClientProxy()
        {
            _deserializerOptions = new JsonSerializerOptions();
            _deserializerOptions.PropertyNameCaseInsensitive = true;
            _deserializerOptions.AllowTrailingCommas = true;
            _deserializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        }

        private readonly bool _enableMetrics;
        private readonly JsonSerializerOptions _serializerOptions;

        private SemaphoreSlim _initSyncLock = new SemaphoreSlim(1);
        private bool _initReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="clientConnection">The underlying client connection that this proxy communicates with.</param>
        /// <param name="schema">The schema this client listens for.</param>
        /// <param name="router">The router component that will send this client event data.</param>
        /// <param name="responseWriter">A response writer instance to
        /// generate query data payloads for outbound data messages.</param>
        /// <param name="logger">The primary logger object to record events to.</param>
        /// <param name="enableMetrics">if set to <c>true</c> any queries this client
        /// executes will have metrics attached.</param>
        public GqltwsClientProxy(
            IClientConnection clientConnection,
            TSchema schema,
            ISubscriptionEventRouter router,
            IQueryResponseWriter<TSchema> responseWriter,
            IGraphEventLogger logger = null,
            bool enableMetrics = false)
            : base(SubscriptionClientId.NewClientId(), schema, clientConnection, router, logger)
        {
            _enableMetrics = enableMetrics;

            Validation.ThrowIfNull(responseWriter, nameof(responseWriter));

            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new GqltwsServerDataMessageConverter(schema, responseWriter));
            _serializerOptions.Converters.Add(new GqltwsServerCompleteMessageConverter());
            _serializerOptions.Converters.Add(new GqltwsServerErrorMessageConverter(schema));
        }

        /// <summary>
        /// Returns a generic error to the client indicating that the last message recieved was unknown or invalid.
        /// </summary>
        /// <param name="lastMessage">The last message that was received that was unprocessable.</param>
        /// <returns>Task.</returns>
        private async Task ResponseToUnknownMessageAsync(GqltwsMessage lastMessage)
        {
            var error = "The last message recieved was unknown or could not be processed " +
                        $"by this server. This connection is configured to use the {GqltwsConstants.PROTOCOL_NAME} " +
                        $"message schema. (messageType: '{lastMessage.Type}')";

            await this.CloseConnectionAsync(
                (ConnectionCloseStatus)GqltwsConstants.CustomCloseEventIds.InvalidMessageType,
                error);
        }

        /// <summary>
        /// Parses the message contents to generate a valid client subscription and adds it to the watched
        /// set for this instance.
        /// </summary>
        /// <param name="message">The message with the subscription details.</param>
        private async Task ExecuteSubscriptionStartRequestAsync(GqltwsClientSubscribeMessage message)
        {
            var result = await this.ExecuteQueryAsync(message.Id, message.Payload, _enableMetrics);
            switch (result.Status)
            {
                case SubscriptionOperationResultType.SubscriptionRegistered:
                    // nothing to do in this case
                    break;

                case SubscriptionOperationResultType.SingleQueryCompleted:

                    // report pre execution syntax errors as single error messages
                    if (result.Messages.Count == 1
                        && result.Messages[0].Code == Constants.ErrorCodes.SYNTAX_ERROR)
                    {
                        var responseMessage = new GqltwsServerErrorMessage(
                              result.Messages[0],
                              message,
                              message.Id);

                        await this.SendMessageAsync(responseMessage).ConfigureAwait(false);
                    }
                    else
                    {
                        // report everything else as a completed request via "next"
                        var responseMessage = new GqltwsServerNextDataMessage(message.Id, result.OperationResult);
                        var completedMessage = new GqltwsSubscriptionCompleteMessage(message.Id);

                        await this.SendMessageAsync(responseMessage).ConfigureAwait(false);
                        await this.SendMessageAsync(completedMessage).ConfigureAwait(false);
                    }

                    break;

                case SubscriptionOperationResultType.IdInUse:
                    var failureMessage = new GqltwsServerErrorMessage(
                        result.Messages?.FirstOrDefault(),
                        lastMessage: message,
                        clientProvidedId: message.Id);

                    await this.SendMessageAsync(failureMessage).ConfigureAwait(false);
                    break;

                case SubscriptionOperationResultType.OperationFailure:

                    if (result.Messages.Count == 1)
                    {
                        await this.SendMessageAsync(
                            new GqltwsServerErrorMessage(
                                result.Messages[0],
                                clientProvidedId: message.Id))
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var response = GraphOperationResult.FromMessages(result.Messages, message.Payload);

                        await this.SendMessageAsync(new GqltwsServerNextDataMessage(message.Id, response))
                            .ConfigureAwait(false);
                        await this.SendMessageAsync(new GqltwsSubscriptionCompleteMessage(message.Id))
                            .ConfigureAwait(false);
                    }

                    break;
            }
        }

        /// <summary>
        /// Attempts to find and remove a subscription with the given client id on the message for the target subscription.
        /// </summary>
        /// <param name="message">The message containing the subscription id to stop.</param>
        /// <returns>Task.</returns>
        private Task ExecuteSubscriptionStopRequestAsync(GqltwsSubscriptionCompleteMessage message)
        {
            var removedSuccessfully = this.ReleaseSubscription(message.Id);

            if (!removedSuccessfully)
            {
                // per spec:
                // Receiving a message (other than Subscribe) with an ID that belongs to an
                // operation that has been previously completed does not constitute an error.
                // It is permissable to simply ignore all unknown IDs without closing the connection.
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected async override Task<GqltwsMessage> DeserializeMessageAsync(Stream stream, CancellationToken cancelToken = default)
        {
            GqltwsMessage recievedMessage;

            try
            {
                // partially deserailize the message to extract the "type" field
                // to determine how to fully deserialize this message
                var partialMessage = await JsonSerializer.DeserializeAsync<GqltwsClientPartialMessage>(
                    stream,
                    _deserializerOptions,
                    cancelToken);
                recievedMessage = partialMessage.Convert();
            }
            catch (Exception ex)
            {
                this.Logger?.UnhandledExceptionEvent(ex);

                string text = "~unreadable~";

                try
                {
                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                            text = reader.ReadToEnd();
                    }
                }
                catch
                {
                }

                // TODO: Capture deserialization errors as a structured event
                recievedMessage = new GqltwsUnknownMessage(text);
            }

            return recievedMessage;
        }

        /// <inheritdoc />
        protected override byte[] SerializeMessage(GqltwsMessage message)
        {
            // how should we serialize this message?
            Type matchedType = typeof(GqltwsMessage);

            if (message != null)
            {
                switch (message.Type)
                {
                    case GqltwsMessageType.NEXT:
                        matchedType = typeof(GqltwsServerNextDataMessage);
                        break;

                    case GqltwsMessageType.COMPLETE:
                        matchedType = typeof(GqltwsSubscriptionCompleteMessage);
                        break;

                    case GqltwsMessageType.ERROR:
                        matchedType = typeof(GqltwsServerErrorMessage);
                        break;
                }
            }

            // graphql is defined to communcate in UTF-8, serialize the result to that
            return JsonSerializer.SerializeToUtf8Bytes(message, matchedType, _serializerOptions);
        }

        /// <inheritdoc />
        protected override async Task ClientMessageReceivedAsync(GqltwsMessage message, CancellationToken cancelToken = default)
        {
            await this.ProcessMessageAsync(message, cancelToken);
        }

        /// <summary>
        /// Handles the MessageRecieved event of a connected client. The connected client
        /// raises this event whenever a message is recieved and successfully parsed from
        /// the under lying websocket.
        /// </summary>
        /// <param name="message">The message that was recieved on the socket.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>TaskMethodBuilder.</returns>
        internal async Task ProcessMessageAsync(GqltwsMessage message, CancellationToken cancelToken = default)
        {
            if (message == null)
                return;

            this.Logger?.ClientProxyMessageReceived(this, message);
            switch (message.Type)
            {
                case GqltwsMessageType.CONNECTION_INIT:
                    await this.AcknowledgeConnectionInitializationMessageAsync();
                    break;

                case GqltwsMessageType.PING:
                    await this.RespondToPingMessageAsync();
                    break;

                case GqltwsMessageType.SUBSCRIBE:
                    await this.ExecuteSubscriptionStartRequestAsync(message as GqltwsClientSubscribeMessage);
                    break;

                case GqltwsMessageType.COMPLETE:
                    await this.ExecuteSubscriptionStopRequestAsync(message as GqltwsSubscriptionCompleteMessage);
                    break;

                // do nothing with a recevied pong message
                case GqltwsMessageType.PONG:
                    break;

                default:
                    await this.ResponseToUnknownMessageAsync(message);
                    break;
            }
        }

        /// <inheritdoc />
        protected override async Task ExecuteKeepAliveAsync(CancellationToken cancelToken = default)
        {
            await this.SendMessageAsync(new GqltwsPingMessage());
        }

        /// <inheritdoc />
        protected override GqltwsMessage CreateDataMessage(string subscriptionId, IQueryOperationResult operationResult)
        {
            return new GqltwsServerNextDataMessage(subscriptionId, operationResult);
        }

        /// <inheritdoc />
        protected override GqltwsMessage CreateCompleteMessage(string subscriptionId)
        {
            return new GqltwsSubscriptionCompleteMessage(subscriptionId);
        }

        /// <summary>
        /// Sends the required startup messages down to the connected client to
        /// acknowledge the connection/protocol.
        /// </summary>
        private async Task AcknowledgeConnectionInitializationMessageAsync()
        {
            await _initSyncLock.WaitAsync();

            try
            {
                if (!_initReceived)
                {
                    _initReceived = true;
                    await this.SendMessageAsync(new GqltwsServerConnectionAckMessage()).ConfigureAwait(false);
                    return;
                }

                // from the spec: https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md
                // -----------------------------
                // If the server receives more than one ConnectionInit message at any given time,
                // the server will close the socket with the event 4429: Too many initialisation requests.
                await this.CloseConnectionAsync(
                    (ConnectionCloseStatus)GqltwsConstants.CustomCloseEventIds.TooManyInitializationRequests,
                    "Too many initialization requests");
            }
            finally
            {
                _initSyncLock.Release();
            }
        }

        /// <inheritdoc />
        protected override async Task InitializationWindowExpiredAsync(CancellationToken token)
        {
            await _initSyncLock.WaitAsync();

            try
            {
                if (_initReceived)
                    return;

                // from the spec: https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md
                // -----------------------------
                // If the server does not recieve the connection init message within a given time
                // the server will close the socket with the event 4408: Connection initialziation timeout
                await this.CloseConnectionAsync(
                    (ConnectionCloseStatus)GqltwsConstants.CustomCloseEventIds.ConnectionInitializationTimeout,
                    "Connection initialization timeout");
            }
            finally
            {
                _initSyncLock.Release();
            }
        }

        /// <summary>
        /// Sends a PONG message down to the connected client to acknowledge a received
        /// PING messsage.
        /// </summary>
        private async Task RespondToPingMessageAsync()
        {
            await this.SendMessageAsync(new GqltwsPongMessage()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _initSyncLock?.Dispose();
                    _initSyncLock = null;
                    _isDisposed = true;
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override string Protocol => GqltwsConstants.PROTOCOL_NAME;
    }
}