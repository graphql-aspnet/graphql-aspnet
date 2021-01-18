// ************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging
{
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extensions to the <see cref="IGraphLogger"/> to process subscription related events.
    /// </summary>
    public static class GraphLoggerExtensions
    {
        /// <summary>
        /// Recorded when the startup services registers a publically available ASP.NET MVC route to which
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
        /// <param name="server">The server which created the client.</param>
        /// <param name="client">The client that was created.</param>
        public static void SubscriptionClientRegistered<TSchema>(
            this IGraphEventLogger logger,
            ISubscriptionServer<TSchema> server,
            ISubscriptionClientProxy client)
            where TSchema : class, ISchema
        {
            logger.Log(
                LogLevel.Debug,
                () => new SubscriptionClientRegisteredLogEntry<TSchema>(server, client));
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
        /// Recorded when a subscription client is no longer connected or otherwise dropped
        /// by ASP.NET. The server will process no more messages from the client.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the server was created for.</typeparam>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="server">The server that was created.</param>
        public static void SubscriptionServerCreated<TSchema>(
            this IGraphEventLogger logger,
            ISubscriptionServer<TSchema> server)
            where TSchema : class, ISchema
        {
            logger.Log(
                LogLevel.Debug,
                () => new SubscriptionServerCreatedLogEntry<TSchema>(server));
        }

        /// <summary>
        /// Recorded by this instance's <see cref="ISubscriptionEventRouter"/> when it receives a new subscription event from
        /// an externally connected source such as a message queue or service or bus. For single server configurations this event
        /// is recorded when an event is passed from the internal publishing queue directly to the <see cref="ISubscriptionEventRouter"/>.
        /// </summary>
        /// <param name="logger">The logger doing the logging.</param>
        /// <param name="eventData">The event data that was received from a data source.</param>
        public static void SubscriptionEventReceived(
            this IGraphEventLogger logger,
            SubscriptionEvent eventData)
        {
            logger.Log(
                LogLevel.Debug,
                () => new SubscriptionEventReceivedLogEntry(eventData));
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
    }
}