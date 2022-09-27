﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Extensions;
    using GraphQL.AspNet.ServerProtocols.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// subscription support for the graphql-ws protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is built for.</typeparam>
    [DebuggerDisplay("Subscriptions = {_subscriptions.Count}")]
    internal sealed class GqltwsClientProxy<TSchema> : ClientProxyBase<TSchema, GqltwsMessage>
        where TSchema : class, ISchema
    {
        private readonly bool _enableMetrics;
        private readonly GqltwsMessageConverterFactory<TSchema> _converterFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="clientConnection">The underlying client connection for graphql-ws to manage.</param>
        /// <param name="logger">The logger to record client level events to, if any.</param>
        /// <param name="enableMetrics">if set to <c>true</c> any queries this client
        /// executes will have metrics attached.</param>
        public GqltwsClientProxy(
            IClientConnection clientConnection,
            IGraphEventLogger logger = null,
            bool enableMetrics = false)
            : base(Guid.NewGuid().ToString(), clientConnection, logger)
        {
            _converterFactory = new GqltwsMessageConverterFactory<TSchema>(this);
            _enableMetrics = enableMetrics;
        }

        /// <summary>
        /// Returns a generic error to the client indicating that the last message recieved was unknown or invalid.
        /// </summary>
        /// <param name="lastMessage">The last message that was received that was unprocessable.</param>
        /// <returns>Task.</returns>
        private async Task ResponseToUnknownMessage(GqltwsMessage lastMessage)
        {
            var error = new GqltwsServerErrorMessage(
                    "The last message recieved was unknown or could not be processed " +
                    "by this server. This GrapQL server is configured to use the graphql-ws " +
                    "message schema.",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: lastMessage,
                    clientProvidedId: lastMessage.Id);

            error.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_NUMBER,
                "Unknown Message Type");

            error.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_URL,
                "https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md");

            await this.SendMessage(error);
            await this.CloseConnection(ClientConnectionCloseStatus.InvalidMessageType);
        }

        /// <summary>
        /// Parses the message contents to generate a valid client subscription and adds it to the watched
        /// set for this instance.
        /// </summary>
        /// <param name="message">The message with the subscription details.</param>
        private async Task ExecuteSubscriptionStartRequest(GqltwsClientSubscribeMessage message)
        {
            var result = await this.ExecuteQuery(message.Id, message.Payload, _enableMetrics);
            switch (result.Status)
            {
                case SubscriptionOperationResultType.SubscriptionRegistered:

                    // nothing to do in this case
                    break;

                case SubscriptionOperationResultType.SingleQueryCompleted:

                    // report syntax errors as single error messages which kills the message stream for the id
                    // allow others to bubble into a result
                    if (result.Messages.Count == 1
                        && result.Messages[0].Code == Constants.ErrorCodes.SYNTAX_ERROR)
                    {
                        var responseMessage = new GqltwsServerErrorMessage(
                              result.Messages[0],
                              message,
                              message.Id);

                        await this.SendMessage(responseMessage).ConfigureAwait(false);
                    }
                    else
                    {
                        // complete the single request
                        var responseMessage = new GqltwsServerNextDataMessage(message.Id, result.OperationResult);
                        var completedMessage = new GqltwsSubscriptionCompleteMessage(message.Id);

                        await this.SendMessage(responseMessage).ConfigureAwait(false);
                        await this.SendMessage(completedMessage).ConfigureAwait(false);
                    }

                    break;

                case SubscriptionOperationResultType.IdInUse:
                    var failureMessage = new GqltwsServerErrorMessage(
                        result.Messages?.FirstOrDefault(),
                        lastMessage: message,
                        clientProvidedId: message.Id);

                    await this.SendMessage(failureMessage).ConfigureAwait(false);
                    break;

                case SubscriptionOperationResultType.OperationFailure:

                    if (result.Messages.Count == 1)
                    {
                        await this.SendMessage(
                            new GqltwsServerErrorMessage(
                                result.Messages[0],
                                clientProvidedId: message.Id))
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var response = GraphOperationResult.FromMessages(result.Messages, message.Payload);

                        await this.SendMessage(new GqltwsServerNextDataMessage(message.Id, response))
                            .ConfigureAwait(false);
                        await this.SendMessage(new GqltwsSubscriptionCompleteMessage(message.Id))
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
        private async Task ExecuteSubscriptionStopRequest(GqltwsSubscriptionCompleteMessage message)
        {
            var removedSuccessfully = this.ReleaseSubscription(message.Id);

            if (!removedSuccessfully)
            {
                var errorMessage = new GqltwsServerErrorMessage(
                    $"No active subscription exists with id '{message.Id}'",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: message,
                    clientProvidedId: message.Id);

                await this
                    .SendMessage(errorMessage)
                    .ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        protected override GqltwsMessage DeserializeMessage(IEnumerable<byte> bytes)
        {
            var text = Encoding.UTF8.GetString(bytes.ToArray());

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            GqltwsMessage recievedMessage;

            try
            {
                // partially deserailize the message to extract the "type" field
                // to determine how to fully deserialize this message
                var partialMessage = JsonSerializer.Deserialize<GqltwsClientPartialMessage>(text, options);
                recievedMessage = partialMessage.Convert();
            }
            catch (Exception ex)
            {
                // TODO: Capture deserialization errors as a structured event
                this.Logger?.EventLogger?.UnhandledExceptionEvent(ex);
                recievedMessage = new GqltwsUnknownMessage(text);
            }

            return recievedMessage;
        }

        /// <inheritdoc />
        protected override byte[] SerializeMessage(GqltwsMessage message)
        {
            // create and register the proper serializer for this message
            var options = new JsonSerializerOptions();
            (var converter, var asType) = _converterFactory.CreateConverter(message);
            options.Converters.Add(converter);

            // graphql is defined to communcate in UTF-8, serialize the result to that
            return JsonSerializer.SerializeToUtf8Bytes(message, asType, options);
        }

        /// <inheritdoc />
        protected override async Task ProcessReceivedMessage(GqltwsMessage message, CancellationToken cancelToken = default)
        {
            await this.ProcessMessage(message, cancelToken);
        }

        /// <summary>
        /// Handles the MessageRecieved event of a connected client. The connected client
        /// raises this event whenever a message is recieved and successfully parsed from
        /// the under lying websocket.
        /// </summary>
        /// <param name="message">The message that was recieved on the socket.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>TaskMethodBuilder.</returns>
        internal async Task ProcessMessage(GqltwsMessage message, CancellationToken cancelToken = default)
        {
            if (message == null)
                return;

            this.Logger?.MessageReceived(message);
            switch (message.Type)
            {
                case GqltwsMessageType.CONNECTION_INIT:
                    await this.AcknowledgeConnectionInitializationMessage();
                    break;

                case GqltwsMessageType.PING:
                    await this.ResponseToPingMessage();
                    break;

                // do nothing with a recevied pong message
                case GqltwsMessageType.PONG:
                    break;

                case GqltwsMessageType.SUBSCRIBE:
                    await this.ExecuteSubscriptionStartRequest(message as GqltwsClientSubscribeMessage);
                    break;

                case GqltwsMessageType.COMPLETE:
                    await this.ExecuteSubscriptionStopRequest(message as GqltwsSubscriptionCompleteMessage);
                    break;

                default:
                    await this.ResponseToUnknownMessage(message);
                    break;
            }
        }

        /// <inheritdoc />
        protected override async Task ExecuteKeepAlive(CancellationToken cancelToken = default)
        {
            await this.SendMessage(new GqltwsPingMessage());
        }

        /// <inheritdoc />
        protected override GqltwsMessage CreateDataMessage(string subscriptionId, IGraphOperationResult operationResult)
        {
            return new GqltwsServerNextDataMessage(subscriptionId, operationResult);
        }

        /// <summary>
        /// Sends the required startup messages down to the connected client to
        /// acknowledge the connection/protocol.
        /// </summary>
        private async Task AcknowledgeConnectionInitializationMessage()
        {
            await this.SendMessage(new GqltwsServerConnectionAckMessage()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a PONG message down to the connected client to acknowledge a received
        /// PING messsage.
        /// </summary>
        private async Task ResponseToPingMessage()
        {
            await this.SendMessage(new GqltwsPongMessage()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override Task SendErrorMessage(IGraphMessage graphMessage, string subscriptionId = null)
        {
            // not supported on graphql-transport-ws
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override string Protocol => GqltwsConstants.PROTOCOL_NAME;
    }
}