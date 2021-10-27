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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubcriptionExecution;

    /// <summary>
    /// A set of constants related to the configuration and processing of subscriptions
    /// through websockets.
    /// </summary>
    public static class SubscriptionConstants
    {
        /// <summary>
        /// A key pointing to the collection within <see cref="GraphQueryExecutionContext"/> that contains
        /// any events that were raised during the query execution.
        /// </summary>
        public const string RAISED_EVENTS_COLLECTION_KEY = "RaisedSubscriptionEvents";

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
            public const string DEFAULT_SUBSCRIPTIONS_ROUTE = Constants.Routing.DEFAULT_HTTP_ROUTE;
        }

        /// <summary>
        /// Common error codes used in graph resolution errors.
        /// </summary>
        public static class ErrorCodes
        {
            /// <summary>
            /// An error raised when a client attempts to process a start request
            /// against a message id that is already in flight for the client.
            /// </summary>
            public const string DUPLICATE_MESSAGE_ID = "DUPLICATE_MESSAGE_ID";
        }

        /// <summary>
        /// Constants pertaining to the execution of subscription type queries.
        /// </summary>
        public static class Execution
        {
            /// <summary>
            /// A key value, pointing to an item in the Items collection of an executed
            /// <see cref="GraphQueryExecutionContext"/> if a subscription was created.  If a subscription
            /// was not created but should have been this value will be null.
            /// </summary>
            public const string CREATED_SUBSCRIPTION = "GRAPHQL_SUBSCRIPTIONS_PIPELINE_CREATED_SUBSCRIPTION";

            /// <summary>
            /// A key value, pointing to an item in the Items collection of an executed
            /// <see cref="SubcriptionExecutionContext"/>of the subscription id on the request
            /// to uniquely identify the created subscription if/when it is created.
            /// </summary>
            public const string SUBSCRIPTION_ID = "GRAPHQL_SUBSCRIPTIONS_ID";

            /// <summary>
            /// A key value, pointing to an item in the Items collection of an executed
            /// <see cref="SubcriptionExecutionContext"/> to a reference of the client that is making the request.
            /// </summary>
            public const string CLIENT = "GRAPHQL_SUBSCRIPTIONS_CLIENT_REFERENCE";
        }

        /// <summary>
        /// Constants pertaining to web socket specifics.
        /// </summary>
        public static class WebSockets
        {
            /// <summary>
            /// The the key value used as the default sub protocol this subscription
            /// server can support.
            /// </summary>
            public const string DEFAULT_SUB_PROTOCOL = "graphql-ws";
        }
    }
}