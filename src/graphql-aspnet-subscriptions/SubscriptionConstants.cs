// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// A set of constants related to the configuration and processing of subscriptions
    /// through websockets.
    /// </summary>
    public static class SubscriptionConstants
    {
        /// <summary>
        /// A collection of constants related to subscription routing.
        /// </summary>
        public static class Routing
        {
            /// <summary>
            /// A meta route that will be replaced with a defined schema route at runtime
            /// when determining the full route for subscriptions.
            /// </summary>
            public const string SCHEMA_ROUTE_KEY = "{schemaRoute}";

            /// <summary>
            /// The default route fragment, appended to the end of a Graphl QL route.
            /// </summary>
            public const string DEFAULT_SUBSCRIPTIONS_ROUTE = SCHEMA_ROUTE_KEY + "/subscriptions";
        }

        /// <summary>
        /// A key pointing to the collection within <see cref="GraphQueryExecutionContext"/> that contains
        /// any events that were raised during the query execution.
        /// </summary>
        public const string RAISED_EVENTS_COLLECTION_KEY = "RaisedSubscriptionEvents";
    }
}