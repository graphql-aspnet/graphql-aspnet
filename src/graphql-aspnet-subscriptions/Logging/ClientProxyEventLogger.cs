﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.ClientProxyLogEntries;
    using GraphQL.AspNet.Schemas.Structural;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A decorator for the <see cref="IGraphEventLogger" /> to capture client proxy specific
    /// events to the log stream.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the client exists for.</typeparam>
    public class ClientProxyEventLogger<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionClientProxy<TSchema> _client;
        private readonly IGraphEventLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProxyEventLogger{TSchema}" /> class.
        /// </summary>
        /// <param name="client">The client being logged.</param>
        /// <param name="logger">The root graph logger to send events to.</param>
        public ClientProxyEventLogger(ISubscriptionClientProxy<TSchema> client, IGraphEventLogger logger)
        {
            _client = Validation.ThrowIfNullOrReturn(client, nameof(client));
            _logger = logger;
        }

        /// <summary>
        /// Recorded when the configured client proxy recieves a new message from the
        /// actual client.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        public void MessageReceived(ILoggableClientProxyMessage message)
        {
            _logger?.Log(
                LogLevel.Trace,
                () => new ClientProxyMessageReceivedLogEntry(_client, message));
        }

        /// <summary>
        /// Recorded when the configured client proxy sends a message downstream to the
        /// actual client.
        /// </summary>
        /// <param name="message">The message that was sent.</param>
        public void MessageSent(ILoggableClientProxyMessage message)
        {
            _logger?.Log(
                LogLevel.Trace,
                () => new ClientProxyMessageSentLogEntry(_client, message));
        }

        /// <summary>
        /// Recorded when the client registers a new subscription
        /// and can send data to the connected client when events are raised.
        /// </summary>
        /// <param name="subscription">The subscription that was created.</param>
        public void SubscriptionCreated(ISubscription subscription)
        {
            _logger?.Log(
                LogLevel.Debug,
                () => new ClientProxySubscriptionCreatedLogEntry(_client, subscription));
        }

        /// <summary>
        /// Recorded when the client drops a subscription
        /// and will no longer send data to it when events ar eraised.
        /// </summary>
        /// <param name="fieldPath">The field path of the event that was received.</param>
        /// <param name="subscriptionsToReceive">The subscriptions set to receive the event.</param>
        public void SubscriptionEventReceived(SchemaItemPath fieldPath, IReadOnlyList<ISubscription> subscriptionsToReceive)
        {
            _logger?.Log(
                LogLevel.Debug,
                () => new ClientProxySubscriptionEventReceived<TSchema>(_client, fieldPath, subscriptionsToReceive));
        }

        /// <summary>
        /// Recorded when the client drops a subscription
        /// and will no longer send data to it when events ar eraised.
        /// </summary>
        /// <param name="subscription">The subscription that was removed.</param>
        public void SubscriptionStopped(ISubscription subscription)
        {
            _logger?.Log(
                LogLevel.Debug,
                () => new ClientProxySubscriptionStoppedLogEntry(_client, subscription));
        }

        /// <summary>
        /// Gets the underlying event logger.
        /// </summary>
        /// <value>The event logger.</value>
        public IGraphEventLogger EventLogger => _logger;
    }
}