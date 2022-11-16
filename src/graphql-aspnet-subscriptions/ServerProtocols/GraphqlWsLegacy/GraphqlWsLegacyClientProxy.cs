// *************************************************************
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Extensions;
    using GraphQL.AspNet.ServerProtocols.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// GraphQL subscription support for GraphqlWsLegacy's graphql-over-websockets protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is built for.</typeparam>
    [DebuggerDisplay("Subscriptions = {_subscriptions.Count}")]
    public class GraphqlWsLegacyClientProxy<TSchema> : SubscriptionClientProxyBase<TSchema, GraphqlWsLegacyMessage>
        where TSchema : class, ISchema
    {
        private static readonly JsonSerializerOptions _deserializeOptions;

        /// <summary>
        /// Initializes static members of the <see cref="GraphqlWsLegacyClientProxy{TSchema}"/> class.
        /// </summary>
        static GraphqlWsLegacyClientProxy()
        {
            _deserializeOptions = new JsonSerializerOptions();
            _deserializeOptions.PropertyNameCaseInsensitive = true;
            _deserializeOptions.AllowTrailingCommas = true;
            _deserializeOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        }

        private readonly bool _enableMetrics;
        private readonly JsonSerializerOptions _serializeOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema this client listens for.</param>
        /// <param name="clientConnection">The underlying client connection that this proxy communicates with.</param>
        /// <param name="router">The router component that will send this client event data.</param>
        /// <param name="protocolName">Name of the protocol this client presents as.</param>
        /// <param name="logger">The primary logger object to record events to.</param>
        /// <param name="enableMetrics">if set to <c>true</c> any queries this client
        /// executes will have metrics attached.</param>
        public GraphqlWsLegacyClientProxy(
            TSchema schema,
            IClientConnection clientConnection,
            ISubscriptionEventRouter router,
            string protocolName,
            IGraphQueryResponseWriter<TSchema> responseWriter,
            IGraphEventLogger logger = null,
            bool enableMetrics = false)
            : base(SubscriptionClientId.NewClientId(), schema, clientConnection, router, logger)
        {
            this.Protocol = Validation.ThrowIfNullWhiteSpaceOrReturn(protocolName, nameof(protocolName));

            Validation.ThrowIfNull(responseWriter, nameof(responseWriter));

            _enableMetrics = enableMetrics;

            _serializeOptions = new JsonSerializerOptions();
            _serializeOptions.Converters.Add(new GraphqlWsLegacyServerDataMessageConverter(schema, responseWriter));
            _serializeOptions.Converters.Add(new GraphqlWsLegacyServerCompleteMessageConverter());
            _serializeOptions.Converters.Add(new GraphqlWsLegacyServerErrorMessageConverter(schema));
        }

        /// <inheritdoc />
        protected override async Task<GraphqlWsLegacyMessage> DeserializeMessage(Stream stream, CancellationToken cancelToken = default)
        {

            GraphqlWsLegacyMessage recievedMessage;

            try
            {
                var partialMessage = await JsonSerializer.DeserializeAsync<GraphqlWsLegacyClientPartialMessage>(stream, _deserializeOptions, cancelToken);
                recievedMessage = partialMessage.Convert();
            }
            catch (Exception ex)
            {
                this.Logger.EventLogger?.UnhandledExceptionEvent(ex);

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

                recievedMessage = new GraphqlWsLegacyUnknownMessage(text);
            }

            return recievedMessage;
        }

        /// <inheritdoc />
        protected override byte[] SerializeMessage(GraphqlWsLegacyMessage message)
        {
            // Does the message need any specialized serialization handling?
            Type asType = typeof(GraphqlWsLegacyMessage);
            switch (message.Type)
            {
                case GraphqlWsLegacyMessageType.DATA:
                    asType = typeof(GraphqlWsLegacyServerDataMessage);
                    break;

                case GraphqlWsLegacyMessageType.COMPLETE:
                    asType = typeof(GraphqlWsLegacyServerCompleteMessage);
                    break;

                case GraphqlWsLegacyMessageType.ERROR:
                    asType = typeof(GraphqlWsLegacyServerErrorMessage);
                    break;
            }

            // graphql is defined to communcate in UTF-8, serialize the result to that
            return JsonSerializer.SerializeToUtf8Bytes(message, asType, _serializeOptions);
        }

        /// <summary>
        /// Handles the MessageRecieved event of the GraphqlWsLegacyClient control. The client raises this event
        /// whenever a message is recieved and successfully parsed from the under lying websocket.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>TaskMethodBuilder.</returns>
        internal Task ProcessMessage(GraphqlWsLegacyMessage message)
        {
            if (message == null)
                return Task.CompletedTask;

            this.Logger?.MessageReceived(message);
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
                        ConnectionCloseStatus.NormalClosure,
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
                    $"by this server. This connection is configured to use the {GraphqlWsLegacyConstants.PROTOCOL_NAME} " +
                    "message schema.",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: lastMessage,
                    clientProvidedId: lastMessage.Id);

            GraphqlWsLegacyError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_NUMBER_KEY,
                "Unknown Message Type");

            GraphqlWsLegacyError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_URL_KEY,
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
            var removedSuccessfully = this.ReleaseSubscription(message.Id);

            if (removedSuccessfully)
            {
                await this.SendMessage(new GraphqlWsLegacyServerCompleteMessage(message.Id))
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
            var result = await this.ExecuteQuery(message.Id, message.Payload, _enableMetrics);

            switch (result.Status)
            {
                case SubscriptionOperationResultType.SubscriptionRegistered:
                    // nothing to do in this case
                    break;

                case SubscriptionOperationResultType.IdInUse:
                    var idInUseMessage = new GraphqlWsLegacyServerErrorMessage(
                        result.Messages?.FirstOrDefault(),
                        lastMessage: message,
                        clientProvidedId: message.Id);
                    await this.SendMessage(idInUseMessage).ConfigureAwait(false);
                    break;

                case SubscriptionOperationResultType.OperationFailure:
                case SubscriptionOperationResultType.SingleQueryCompleted:
                    GraphqlWsLegacyMessage responseMessage = null;

                    // report syntax errors as error messages
                    // allow others to bubble into a full results
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
                        responseMessage = new GraphqlWsLegacyServerDataMessage(message.Id, result.OperationResult);
                    }

                    await this.SendMessage(responseMessage).ConfigureAwait(false);
                    await this.SendMessage(new GraphqlWsLegacyServerCompleteMessage(message.Id)).ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Sends the required startup messages down to the connected client to acknowledge the connection/protocol.
        /// </summary>
        private async Task AcknowledgeNewConnection()
        {
            // protocol dictates the messages must be sent in this order
            // await each send before attempting the next one
            await this.SendMessage(new GraphqlWsLegacyServerAckOperationMessage()).ConfigureAwait(false);

            if (this.IsKeepAliveEnabled)
                await this.SendMessage(new GraphqlWsLegacyKeepAliveOperationMessage()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async Task ExecuteKeepAlive(CancellationToken cancelToken = default)
        {
            await this.SendMessage(new GraphqlWsLegacyKeepAliveOperationMessage());
        }

        /// <inheritdoc />
        protected override async Task ClientMessageReceived(GraphqlWsLegacyMessage message, CancellationToken cancelToken = default)
        {
            await this.ProcessMessage(message);
        }

        /// <inheritdoc />
        protected override GraphqlWsLegacyMessage CreateDataMessage(string subscriptionId, IGraphOperationResult operationResult)
        {
            return new GraphqlWsLegacyServerDataMessage(subscriptionId, operationResult);
        }

        /// <inheritdoc />
        protected override GraphqlWsLegacyMessage CreateCompleteMessage(string subscriptionId)
        {
            return new GraphqlWsLegacyServerCompleteMessage(subscriptionId);
        }

        /// <inheritdoc />
        public override string Protocol { get; }
    }
}