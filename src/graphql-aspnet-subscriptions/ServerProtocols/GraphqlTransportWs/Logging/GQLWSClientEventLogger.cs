// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging.Events;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A decorator for the <see cref="IGraphLogger" /> to capture graphql-ws specific
    /// events to the log stream.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the logger exists for.</typeparam>
    internal class GqltwsClientEventLogger<TSchema>
        where TSchema : class, ISchema
    {
        private readonly GqltwsClientProxy<TSchema> _client;
        private readonly IGraphEventLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientEventLogger{TSchema}" /> class.
        /// </summary>
        /// <param name="client">The client being logged.</param>
        /// <param name="logger">The root graph logger to send graphql-ws events to.</param>
        public GqltwsClientEventLogger(GqltwsClientProxy<TSchema> client, IGraphEventLogger logger)
        {
            _client = Validation.ThrowIfNullOrReturn(client, nameof(client));
            _logger = Validation.ThrowIfNullOrReturn(logger, nameof(logger));
        }

        /// <summary>
        /// Recorded when the configured client proxy recieves a new message from the
        /// actual client.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        public void MessageReceived(GqltwsMessage message)
        {
            _logger.Log(
                LogLevel.Trace,
                () => new GqltwsClientMessageReceivedLogEntry(_client, message));
        }

        /// <summary>
        /// Recorded when the configured client proxy sends a message downstream to the
        /// actual client.
        /// </summary>
        /// <param name="message">The message that was sent.</param>
        public void MessageSent(GqltwsMessage message)
        {
            _logger.Log(
                LogLevel.Trace,
                () => new GqltwsClientMessageSentLogEntry(_client, message));
        }

        /// <summary>
        /// Recorded when the client registers a new subscription
        /// and can send data to the connected client when events are raised.
        /// </summary>
        /// <param name="subscription">The subscription that was created.</param>
        public void SubscriptionCreated(ISubscription subscription)
        {
            _logger.Log(
                LogLevel.Debug,
                () => new GqltwsClientSubscriptionCreatedLogEntry(_client, subscription));
        }

        /// <summary>
        /// Recorded when the client drops a subscription
        /// and will no longer send data to it when events ar eraised.
        /// </summary>
        /// <param name="fieldPath">The field path of the event that was received.</param>
        /// <param name="subscriptionsToReceive">The subscriptions set to receive the event.</param>
        public void SubscriptionEventReceived(SchemaItemPath fieldPath, IReadOnlyList<ISubscription> subscriptionsToReceive)
        {
            _logger.Log(
                LogLevel.Debug,
                () => new GqltwsClientSubscriptionEventReceived<TSchema>(_client, fieldPath, subscriptionsToReceive));
        }

        /// <summary>
        /// Recorded when the client drops a subscription
        /// and will no longer send data to it when events ar eraised.
        /// </summary>
        /// <param name="subscription">The subscription that was removed.</param>
        public void SubscriptionStopped(ISubscription subscription)
        {
            _logger.Log(
                LogLevel.Debug,
                () => new GqltwsClientSubscriptionStoppedLogEntry(_client, subscription));
        }

        /// <summary>
        /// Gets the underlying event logger this logger is based on.
        /// </summary>
        /// <value>The event logger.</value>
        public IGraphEventLogger EventLogger => _logger;
    }
}