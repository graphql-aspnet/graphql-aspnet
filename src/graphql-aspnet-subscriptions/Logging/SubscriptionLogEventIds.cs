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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A set of event Ids for subscription related events.
    /// </summary>
    public static class SubscriptionLogEventIds
    {
        private const int BASE_SUBSCRIPTION_EVENT_ID = 87000;

        /// <summary>
        /// The schema subscription route registered event.
        /// </summary>
        public static EventId SchemaRouteRegistered = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 120, "GraphQL Schema Subscription Route Registered");

        /// <summary>
        /// A new client has been registered to a subscription server.
        /// </summary>
        public static EventId SubscriptionClientRegistered = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 200, "GraphQL Subscription Client Registered");

        /// <summary>
        /// A client has been dropped and is no longer being monitored.
        /// </summary>
        public static EventId SubscriptionClientDropped = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 300, "GraphQL Subscription Client Dropped");

        /// <summary>
        /// A new server has been been created by the runtime.
        /// </summary>
        public static EventId SubscriptionServerCreated = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 400, "GraphQL Subscription Server Created");

        /// <summary>
        /// A new event was published by an ASP.NET server instance.
        /// </summary>
        public static EventId GlobalEventPublished = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 500, "GraphQL Event Published");

        /// <summary>
        /// A new event was received by an ASP.NET server instance.
        /// </summary>
        public static EventId GlobalEventReceived = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 510, "GraphQL Event Received");

        /// <summary>
        /// A server component registered a request with the listener to start
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStarted = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 520, "GraphQL Server Subscription Event Monitor Started");

        /// <summary>
        /// A graphql-ws server component received a dispatched subscription event from this ASP.NET server
        /// instance's global listener.
        /// </summary>
        public static EventId ServerSubcriptionEventReceived = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 530, "GraphQL Server Subscription Event Received");

        /// <summary>
        /// A server component unregistered a request with the listener and will stop
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStopped = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 540, "GraphQL Server Subscription Event Monitor Stopped");
    }
}