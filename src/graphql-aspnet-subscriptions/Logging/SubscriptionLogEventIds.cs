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
    }
}