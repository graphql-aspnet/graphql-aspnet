// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A collection of event ids for log entries related to the <see cref="GQLWSClientProxy{TSchema}"/>.
    /// </summary>
    public static class GQLWSLogEventIds
    {
        /// <summary>
        /// Gets or sets the base event number indicating an event is from the graphql-ws server implementation.
        /// <remarks>
        /// The value is global for all schemas and should be set prior to calling any graphql
        /// setup functions.
        /// </remarks>
        /// </summary>
        /// <value>The root graphql-ws event identifier.</value>
        public static int ROOT_GRAPHQLWS_EVENT_ID { get; set; } = 88000;

        /// <summary>
        /// A graphql-ws client proxy received a new graphql-ws formatted message from its connected client.
        /// </summary>
        public static EventId ClientMessageReceived = new EventId(ROOT_GRAPHQLWS_EVENT_ID + 110, "GraphQL-WS Client Message Received");

        /// <summary>
        /// A graphql-ws client proxy generated a graphql-ws message and sent it to its connected client.
        /// </summary>
        public static EventId ClientMessageSent = new EventId(ROOT_GRAPHQLWS_EVENT_ID + 120, "GraphQL-WS Client Message Sent");

        /// <summary>
        /// A graphql-ws client proxy began a monitoring a new subscription.
        /// </summary>
        public static EventId ClientSubscriptionStarted = new EventId(ROOT_GRAPHQLWS_EVENT_ID + 130, "GraphQL-WS Client Subscription Started");

        /// <summary>
        /// A graphql-ws client proxy stopped monitoring an existing subscription.
        /// </summary>
        public static EventId ClientSubscriptionStopped = new EventId(ROOT_GRAPHQLWS_EVENT_ID + 140, "GraphQL-WS Client Subscription Stopped");

        /// <summary>
        /// A graphql-ws client proxy has received an event from its server component and is executing it
        /// against any monitored subscriptions for its connected client.
        /// </summary>
        public static EventId ClientSubscriptionEventRecieved = new EventId(ROOT_GRAPHQLWS_EVENT_ID + 150, "GraphQL-WS Client Subscription Event Received");
    }
}