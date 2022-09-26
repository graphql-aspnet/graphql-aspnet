// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Logging
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A collection of event ids for log entries related to the <see cref="ApolloSubscriptionServer{TSchema}"/>.
    /// </summary>
    public static class ApolloLogEventIds
    {
        /// <summary>
        /// Gets or sets the base event number indicating an event is from the apollo graphql server implementation.
        /// <remarks>
        /// The value is global for all schemas and should be set prior to calling any graphql
        /// setup functions.
        /// </remarks>
        /// </summary>
        /// <value>The root apollo event identifier.</value>
        public static int ROOT_APOLLO_EVENT_ID { get; set; } = 99000;

        /// <summary>
        /// An apollo client proxy received a new apollo formatted message from its connected client.
        /// </summary>
        public static EventId ClientMessageReceived = new EventId(ROOT_APOLLO_EVENT_ID + 110, "Apollo Client Message Received");

        /// <summary>
        /// An apollo client proxy generated a apollo message and sent it to its connected client.
        /// </summary>
        public static EventId ClientMessageSent = new EventId(ROOT_APOLLO_EVENT_ID + 120, "Apollo Client Message Sent");

        /// <summary>
        /// An apollo client proxy began a monitoring a new subscription.
        /// </summary>
        public static EventId ClientSubscriptionStarted = new EventId(ROOT_APOLLO_EVENT_ID + 130, "Apollo Client Subscription Started");

        /// <summary>
        /// An apollo client proxy stopped monitoring an existing subscription.
        /// </summary>
        public static EventId ClientSubscriptionStopped = new EventId(ROOT_APOLLO_EVENT_ID + 140, "Apollo Client Subscription Stopped");

        /// <summary>
        /// An apollo client proxy has received an event from its server component and is executing it
        /// against any monitored subscriptions for its connected client.
        /// </summary>
        public static EventId ClientSubscriptionEventRecieved = new EventId(ROOT_APOLLO_EVENT_ID + 150, "Apollo Client Subscription Event Received");
    }
}