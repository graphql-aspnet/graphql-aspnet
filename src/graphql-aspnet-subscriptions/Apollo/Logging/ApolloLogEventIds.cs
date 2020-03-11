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
        /// An apollo client proxy received a new apollo formatted message from its connected client.
        /// </summary>
        public static EventId ClientMessageReceived = new EventId(BASE_APOLLO_EVENT_ID + 110, "Apollo Client Message Received");

        /// <summary>
        /// An apollo client proxy generated a apollo message and sent it to its connected client.
        /// </summary>
        public static EventId ClientMessageSent = new EventId(BASE_APOLLO_EVENT_ID + 120, "Apollo Client Message Sent");

        /// <summary>
        /// An apollo client proxy began a monitoring a new subscription.
        /// </summary>
        public static EventId ClientSubscriptionStarted = new EventId(BASE_APOLLO_EVENT_ID + 130, "Apollo Client Subscription Started");

        /// <summary>
        /// An apollo client proxy stopped monitoring an existing subscription.
        /// </summary>
        public static EventId ClientSubscriptionStopped = new EventId(BASE_APOLLO_EVENT_ID + 140, "Apollo Client Subscription Stopped");

        /// <summary>
        /// An apollo client proxy has received an event from its server component and is executing it
        /// against any monitored subscriptions for its connected client.
        /// </summary>
        public static EventId ClientSubscriptionEventRecieved = new EventId(BASE_APOLLO_EVENT_ID + 150, "Apollo Client Subscription Event Received");

        /// <summary>
        /// An apollo server component registered a request with the listener to start
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStarted = new EventId(BASE_APOLLO_EVENT_ID + 200, "Apollo Server Subscription Event Monitor Started");

        /// <summary>
        /// An apollo server component unregistered a request with the listener and will stop
        /// receiving events for a given subscription event.
        /// </summary>
        public static EventId ServerSubscriptionEventMonitorStopped = new EventId(BASE_APOLLO_EVENT_ID + 210, "Apollo Server Subscription Event Monitor Stopped");

        /// <summary>
        /// An apollo server component received a dispatched subscription event from this ASP.NET server
        /// instance's global listener.
        /// </summary>
        public static EventId ServerSubcriptionEventReceived = new EventId(BASE_APOLLO_EVENT_ID + 300, "Apollo Server Subscription Event Received");
    }
}