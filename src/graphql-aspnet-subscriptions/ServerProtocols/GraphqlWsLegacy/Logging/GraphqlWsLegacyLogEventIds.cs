// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Logging
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A collection of event ids for log entries related to the <see cref="GraphqlWsLegacySubscriptionServer{TSchema}"/>.
    /// </summary>
    public static class GraphqlWsLegacyLogEventIds
    {
        /// <summary>
        /// Gets or sets the base event number indicating an event is from the GraphqlWsLegacy graphql server implementation.
        /// <remarks>
        /// The value is global for all schemas and should be set prior to calling any graphql
        /// setup functions.
        /// </remarks>
        /// </summary>
        /// <value>The root GraphqlWsLegacy event identifier.</value>
        public static int ROOT_GraphqlWsLegacy_EVENT_ID { get; set; } = 99000;

        /// <summary>
        /// An GraphqlWsLegacy client proxy received a new GraphqlWsLegacy formatted message from its connected client.
        /// </summary>
        public static EventId ClientMessageReceived = new EventId(ROOT_GraphqlWsLegacy_EVENT_ID + 110, "GraphqlWsLegacy Client Message Received");

        /// <summary>
        /// An GraphqlWsLegacy client proxy generated a GraphqlWsLegacy message and sent it to its connected client.
        /// </summary>
        public static EventId ClientMessageSent = new EventId(ROOT_GraphqlWsLegacy_EVENT_ID + 120, "GraphqlWsLegacy Client Message Sent");

        /// <summary>
        /// An GraphqlWsLegacy client proxy began a monitoring a new subscription.
        /// </summary>
        public static EventId ClientSubscriptionStarted = new EventId(ROOT_GraphqlWsLegacy_EVENT_ID + 130, "GraphqlWsLegacy Client Subscription Started");

        /// <summary>
        /// An GraphqlWsLegacy client proxy stopped monitoring an existing subscription.
        /// </summary>
        public static EventId ClientSubscriptionStopped = new EventId(ROOT_GraphqlWsLegacy_EVENT_ID + 140, "GraphqlWsLegacy Client Subscription Stopped");

        /// <summary>
        /// An GraphqlWsLegacy client proxy has received an event from its server component and is executing it
        /// against any monitored subscriptions for its connected client.
        /// </summary>
        public static EventId ClientSubscriptionEventRecieved = new EventId(ROOT_GraphqlWsLegacy_EVENT_ID + 150, "GraphqlWsLegacy Client Subscription Event Received");
    }
}