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
    }
}