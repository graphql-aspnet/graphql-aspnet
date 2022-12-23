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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A set of constants related to the configuration and processing of subscriptions
    /// through websockets.
    /// </summary>
    public static class SubscriptionConstants
    {
        /// <summary>
        /// The default number of concurrent recievers that the event router will send a
        /// received event to at one time.
        /// </summary>
        public const int DEFAULT_MAX_CONCURRENT_SUBSCRIPTION_RECEIVERS = 500;

        /// <summary>
        /// A collection of default settings related to alerts raised by the subscription server.
        /// </summary>
        public static class Alerts
        {
            /// <summary>
            /// Gets the default dispatch queue alert settings used if none are provided by
            /// the user.
            /// </summary>
            /// <value>The default alert settings for the dispatch queue.</value>
            public static ISubscriptionClientDispatchQueueAlertSettings DefaultDispatchQueueAlertSettings { get; }

            /// <summary>
            /// Initializes static members of the <see cref="Alerts"/> class.
            /// </summary>
            static Alerts()
            {
                var settings = new SubscriptionClientDispatchQueueAlertSettings();
                settings.AddThreshold(
                    LogLevel.Critical,
                    10_000,
                    System.TimeSpan.FromMinutes(5));

                DefaultDispatchQueueAlertSettings = settings;
            }
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
        /// Constants pertaining to the context data collection of the various middleeware context types.
        /// </summary>
        public static class ContextDataKeys
        {
            /// <summary>
            /// A key pointing to the collection within <see cref="QueryExecutionContext"/> that contains
            /// any events that were raised during the query execution.
            /// </summary>
            public const string RAISED_EVENTS_COLLECTION = "GraphqlAspNet:Subscriptions:RaisedSubscriptionEvents";

            /// <summary>
            /// A key added to the items collection of a request of a subscription
            /// indicating that the request should be dropped/skipped.
            /// </summary>
            public const string SKIP_EVENT = "GraphqlAspNet:ControlKeys:Subscriptions:SkipSubscriptionEvent";

            /// <summary>
            /// A key added to the items collection of a request of a subscription
            /// indicating that the subscription should be closed upon completion of the event.
            /// </summary>
            public const string COMPLETE_SUBSCRIPTION = "GraphqlAspNet:ControlKeys:Subscriptions:CompleteSubscription";
        }

        /// <summary>
        /// Constants pertaining to web socket specifics.
        /// </summary>
        public static class WebSockets
        {
            /// <summary>
            /// The name of the header on an http request that will contain the requested
            /// graphql messaging protocol.
            /// </summary>
            public const string WEBSOCKET_PROTOCOL_HEADER = "sec-websocket-protocol";
        }
    }
}