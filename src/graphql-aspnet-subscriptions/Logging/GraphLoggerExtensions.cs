// *************************************************************
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
        public static void SchemaSubscriptionRouteRegistered<TSchema>(this IGraphLogger logger, string routePath)
            where TSchema : class, ISchema
        {
            logger.Log(
                LogLevel.Debug,
                new SchemaSubscriptionRouteRegisteredLogEntry<TSchema>(routePath));
        }
    }
}