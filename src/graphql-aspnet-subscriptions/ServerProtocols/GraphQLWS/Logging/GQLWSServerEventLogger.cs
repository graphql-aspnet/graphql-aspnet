﻿// *************************************************************
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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging.Events;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A decorator for the <see cref="IGraphLogger" /> to capture graphql-ws specific
    /// events to the log stream.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the client exists for.</typeparam>
    public class GQLWSServerEventLogger<TSchema>
        where TSchema : class, ISchema
    {
        private readonly GQLWSSubscriptionServer<TSchema> _server;
        private readonly IGraphEventLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSServerEventLogger{TSchema}" /> class.
        /// </summary>
        /// <param name="server">The server being logged.</param>
        /// <param name="logger">The root graph logger to send graphql-ws events to.</param>
        public GQLWSServerEventLogger(GQLWSSubscriptionServer<TSchema> server, IGraphEventLogger logger)
        {
            _server = Validation.ThrowIfNullOrReturn(server, nameof(server));
            _logger = Validation.ThrowIfNullOrReturn(logger, nameof(logger));
        }

        /// <summary>
        /// Recorded when an graphql-ws server component registers a request with the configured subscription event
        /// listener for this ASP.NET server instance. This log entry is recorded when the first connected client
        /// begins a subscription for an event.
        /// </summary>
        /// <param name="eventName">Name of the event that is now being monitored.</param>
        public void EventMonitorStarted(SubscriptionEventName eventName)
        {
            _logger.Log(
                LogLevel.Trace,
                () => new GQLWSServerEventMonitorStartedLogEntry<TSchema>(_server, eventName));
        }

        /// <summary>
        /// Recorded when an graphql-ws server component unregistered a subscription event on the configured
        /// listener for this ASP.NET server instance. This log entry is recorded when the last connected client
        /// stops its last subscription for a given event.
        /// </summary>
        /// <param name="eventName">Name of the event that is no longer being monitored.</param>
        public void EventMonitorEnded(SubscriptionEventName eventName)
        {
            _logger.Log(
                LogLevel.Trace,
                () => new GQLWSServerEventMonitorEndedLogEntry<TSchema>(_server, eventName));
        }

        /// <summary>
        /// Recorded when an graphql-ws Subscription Server instance receives an event
        /// from the listener configured for this ASP.NET server instance.
        /// </summary>
        /// <param name="eventRecieved">The event that was recieved from the global listener.</param>
        /// <param name="clientsToReceive">The filtered list of clients that will receive the event
        /// from the server.</param>
        public void EventReceived(
              SubscriptionEvent eventRecieved,
              IReadOnlyList<GQLWSClientProxy<TSchema>> clientsToReceive)
        {
            _logger.Log(
                LogLevel.Trace,
                () => new GQLWSServerSubscriptionEventReceived<TSchema>(_server, eventRecieved, clientsToReceive));
        }
    }
}