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
        private const int BASE_APOLLO_EVENT_ID = 88000;

        /// <summary>
        /// A new apollo message was received by an apollo client.
        /// </summary>
        public static EventId NewMessageReceived = new EventId(BASE_APOLLO_EVENT_ID + 120, "Apollo Message Received");

        /// <summary>
        /// An apollo client began a monitoring a new subscription.
        /// </summary>
        public static EventId SubscriptionStarted = new EventId(BASE_APOLLO_EVENT_ID + 130, "Apollo Subscription Started");

        /// <summary>
        /// An apollo client stopped monitoring an existing subscription.
        /// </summary>
        public static EventId SubscriptionStopped = new EventId(BASE_APOLLO_EVENT_ID + 140, "Apollo Subscription Stopped");

        /// <summary>
        /// An apollo server component registered a request with the listener to start
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStarted = new EventId(BASE_APOLLO_EVENT_ID + 200, "Apollo Server Subscription Event Started");

        /// <summary>
        /// An apollo server component unregistered a request with the listener and will stop
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStopped = new EventId(BASE_APOLLO_EVENT_ID + 210, "Apollo Server Subscription Event Stopped");
    }
}