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
        /// A server component received a dispatched subscription event from this ASP.NET server
        /// instance's global listener.
        /// </summary>
        public static EventId ServerSubcriptionEventReceived = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 530, "GraphQL Server Subscription Event Received");

        /// <summary>
        /// A server component unregistered a request with the listener and will stop
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStopped = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 540, "GraphQL Server Subscription Event Monitor Stopped");

        /// <summary>
        /// A client attempted to connect to the server with an unsupported messaging protocol
        /// for the target schema.
        /// </summary>
        public static EventId UnsupportedClientProtocol = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 640, "GraphQL Server Unsupported Client Protocol");

        /// <summary>
        /// An GraphqlWsLegacy client proxy received a new GraphqlWsLegacy formatted message from its connected client.
        /// </summary>
        public static EventId ClientMessageReceived = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 710, "GraphqlWsLegacy Client Message Received");

        /// <summary>
        /// An GraphqlWsLegacy client proxy generated a GraphqlWsLegacy message and sent it to its connected client.
        /// </summary>
        public static EventId ClientMessageSent = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 720, "GraphqlWsLegacy Client Message Sent");

        /// <summary>
        /// An GraphqlWsLegacy client proxy began a monitoring a new subscription.
        /// </summary>
        public static EventId ClientSubscriptionStarted = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 730, "GraphqlWsLegacy Client Subscription Started");

        /// <summary>
        /// An GraphqlWsLegacy client proxy stopped monitoring an existing subscription.
        /// </summary>
        public static EventId ClientSubscriptionStopped = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 740, "GraphqlWsLegacy Client Subscription Stopped");

        /// <summary>
        /// An GraphqlWsLegacy client proxy has received an event from its server component and is executing it
        /// against any monitored subscriptions for its connected client.
        /// </summary>
        public static EventId ClientSubscriptionEventRecieved = new EventId(BASE_SUBSCRIPTION_EVENT_ID + 750, "GraphqlWsLegacy Client Subscription Event Received");
    }
}