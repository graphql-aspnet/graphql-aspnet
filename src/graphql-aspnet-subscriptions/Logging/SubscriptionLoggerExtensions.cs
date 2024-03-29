﻿// ************************************************************
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
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extensions to the <see cref="IGraphLogger"/> to process subscription related events.
    /// </summary>
    public static class SubscriptionLoggerExtensions
    {
        /// <summary>
        /// Recorded when the startup services registers a publically available ASP.NET route to which
        /// end users can intiate a websocket request through which subscriptions can be established.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the route was registered for.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="routePath">The relative route path (e.g. '/graphql').</param>
        public static void SchemaSubscriptionRouteRegistered<TSchema>(
            this IGraphEventLogger logger,
            string routePath)
            where TSchema : class, ISchema
        {
            logger.Log(
                LogLevel.Debug,
                () => new SchemaSubscriptionRouteRegisteredLogEntry<TSchema>(routePath));
        }

        /// <summary>
        /// Recorded when a new client is registered against a subscription server and
        /// the graphql server begins monitoring it for messages.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the client was registered for.</typeparam>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="client">The client that was created.</param>
        public static void SubscriptionClientRegistered<TSchema>(
            this IGraphEventLogger logger,
            ISubscriptionClientProxy client)
            where TSchema : class, ISchema
        {
            logger.Log(
                LogLevel.Debug,
                () => new SubscriptionClientRegisteredLogEntry<TSchema>(client));
        }

        /// <summary>
        /// Recorded when a subscription client is no longer connected or otherwise dropped
        /// by ASP.NET. The server will process no more messages from the client.
        /// </summary>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="client">The client that was dropped and is being cleaned up.</param>
        public static void SubscriptionClientDropped(
            this IGraphEventLogger logger,
            ISubscriptionClientProxy client)
        {
            logger.Log(
                LogLevel.Debug,
                () => new SubscriptionClientDroppedLogEntry(client));
        }

        /// <summary>
        /// Recorded by this instance's <see cref="ISubscriptionEventRouter"/> when it receives a new subscription event from
        /// an externally connected source such as a message queue or service or bus. For single server configurations this event
        /// is recorded when an event is passed from the internal publishing queue directly to the <see cref="ISubscriptionEventRouter"/>.
        /// </summary>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="eventData">The event data that was received from a data source.</param>
        public static void SubscriptionEventReceived(
            this ILogger logger,
            SubscriptionEvent eventData)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            // note this event is ILogger not IGraphEventLogger
            // its consumed from a global singleton (IGraphEventLogger is a scoped service due to the scope id property)
            var data = new SubscriptionEventReceivedLogEntry(eventData);
            logger.Log(
                LogLevel.Debug,
                SubscriptionLogEventIds.GlobalEventReceived,
                data,
                null,
                (s, e) => s.ToString());
        }

        /// <summary>
        /// Recorded by this instance's <see cref="ISubscriptionEventRouter" /> when it receives a new subscription event from
        /// an externally connected source such as a message queue or service or bus. For single server configurations this event
        /// is recorded when an event is passed from the internal publishing queue directly to the <see cref="ISubscriptionEventRouter" />.
        /// </summary>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="eventCount">The current event count of the dispatch queue.</param>
        /// <param name="queueCountThreshold">The threshold configuration object that caused
        /// this event to be raised.</param>
        public static void SubscriptionEventDispatchQueueThresholdReached(
            this ILogger logger,
            int eventCount,
            SubscriptionEventAlertThreshold queueCountThreshold)
        {
            if (queueCountThreshold == null)
                return;

            if (!logger.IsEnabled(queueCountThreshold.LogLevel))
                return;

            // note this event is ILogger not IGraphEventLogger
            // its consumed from a global singleton (IGraphEventLogger is a scoped service due to the scope id property)
            var alertMessage = new SubscriptionEventDispatchQueueAlertLogEntry(
                queueCountThreshold.SubscriptionEventCountThreshold,
                eventCount,
                queueCountThreshold.CustomMessage);

            logger.Log(
                    queueCountThreshold.LogLevel,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    alertMessage,
                    null,
                    (s, e) => s.ToString());
        }

        /// <summary>
        /// Recorded when this server successfully publishes a subscription event to the configured <see cref="ISubscriptionEventPublisher"/>
        /// for this instance.
        /// </summary>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="eventData">The event data that was published.</param>
        public static void SubscriptionEventPublished(
            this IGraphEventLogger logger,
            SubscriptionEvent eventData)
        {
            logger.Log(
                LogLevel.Debug,
                () => new SubscriptionEventPublishedLogEntry(eventData));
        }

        /// <summary>
        /// Recorded when a client attempts to connect to a subscription server, for a specific schema,
        /// using a protocol explicitly not supported by that schema.
        /// </summary>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="schema">The schema requested.</param>
        /// <param name="protocol">The protocol that was attempted.</param>
        public static void UnsupportedClientProtocol(
            this IGraphEventLogger logger,
            ISchema schema,
            string protocol)
        {
            logger.Log(
                LogLevel.Warning,
                () => new UnsupportClientSubscriptionProtocolLogEntry(schema, protocol));
        }

        /// <summary>
        /// Recorded when the configured client proxy recieves a new message from the
        /// actual client.
        /// </summary>
        /// <param name="eventLogger">The event logger to record the log entry to.</param>
        /// <param name="clientProxy">The client proxy recording the event.</param>
        /// <param name="message">The message that was received.</param>
        public static void ClientProxyMessageReceived(this IGraphEventLogger eventLogger, ISubscriptionClientProxy clientProxy, ILoggableClientProxyMessage message)
        {
            eventLogger?.Log(
                LogLevel.Trace,
                () => new ClientProxyMessageReceivedLogEntry(clientProxy, message));
        }

        /// <summary>
        /// Recorded when the configured client proxy sends a message downstream to the
        /// actual client.
        /// </summary>
        /// <param name="eventLogger">The event logger to record the log entry to.</param>
        /// <param name="clientProxy">The client proxy recording the event.</param>
        /// <param name="message">The message that was sent.</param>
        public static void ClientProxyMessageSent(this IGraphEventLogger eventLogger, ISubscriptionClientProxy clientProxy, ILoggableClientProxyMessage message)
        {
            eventLogger?.Log(
                LogLevel.Trace,
                () => new ClientProxyMessageSentLogEntry(clientProxy, message));
        }

        /// <summary>
        /// Recorded when the client registers a new subscription
        /// and can send data to the connected client when events are raised.
        /// </summary>
        /// <param name="eventLogger">The event logger to record the log entry to.</param>
        /// <param name="clientProxy">The client proxy recording the event.</param>
        /// <param name="subscription">The subscription that was created.</param>
        public static void ClientProxySubscriptionCreated(this IGraphEventLogger eventLogger, ISubscriptionClientProxy clientProxy, ISubscription subscription)
        {
            eventLogger?.Log(
                LogLevel.Debug,
                () => new ClientProxySubscriptionCreatedLogEntry(clientProxy, subscription));
        }

        /// <summary>
        /// Recorded when the client drops a subscription
        /// and will no longer send data to it when events ar eraised.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema under which the subscription event was received.</typeparam>
        /// <param name="eventLogger">The event logger to record the log entry to.</param>
        /// <param name="clientProxy">The client proxy recording the event.</param>
        /// <param name="fieldPath">The field path of the event that was received.</param>
        /// <param name="subscriptionsToReceive">The subscriptions set to receive the event.</param>
        public static void ClientProxySubscriptionEventReceived<TSchema>(this IGraphEventLogger eventLogger, ISubscriptionClientProxy clientProxy, SchemaItemPath fieldPath, IReadOnlyList<ISubscription> subscriptionsToReceive)
            where TSchema : class, ISchema
        {
            eventLogger?.Log(
                LogLevel.Debug,
                () => new ClientProxySubscriptionEventReceived<TSchema>(clientProxy, fieldPath, subscriptionsToReceive));
        }

        /// <summary>
        /// Recorded when the client drops a subscription
        /// and will no longer send data to it when events ar eraised.
        /// </summary>
        /// <param name="eventLogger">The event logger to record the log entry to.</param>
        /// <param name="clientProxy">The client proxy recording the event.</param>
        /// <param name="subscription">The subscription that was removed.</param>
        public static void ClientProxySubscriptionStopped(this IGraphEventLogger eventLogger, ISubscriptionClientProxy clientProxy, ISubscription subscription)
        {
            eventLogger?.Log(
                LogLevel.Debug,
                () => new ClientProxySubscriptionStoppedLogEntry(clientProxy, subscription));
        }
    }
}