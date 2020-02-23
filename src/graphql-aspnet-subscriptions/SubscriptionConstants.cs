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
        /// Initializes static members of the <see cref="SubscriptionConstants"/> class.
        /// </summary>
        static SubscriptionConstants()
        {
        }

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

        /// <summary>
        /// Constnats pertaining to the execution of subscription type queries
        /// </summary>
        public static class Execution
        {
            /// <summary>
            /// A key value, pointing to an item in the Items collection of an executed
            /// <see cref="GraphQueryExecutionContext"/> if a subscription was created.  If a subscription
            /// was not created but should have been this value will be null.
            /// </summary>
            public const string CREATED_SUBSCRIPTION = "PIPELINE_CREATED_SUBSCRIPTION";
        }
    }
}