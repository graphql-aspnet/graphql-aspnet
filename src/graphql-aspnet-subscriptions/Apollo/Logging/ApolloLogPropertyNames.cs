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
    /// <summary>
    /// A set of known property names for apollo related log entries.
    /// </summary>
    public static class ApolloLogPropertyNames
    {
        /// <summary>
        /// The 'type' field of an apollo message.
        /// </summary>
        public const string MESSAGE_TYPE = "apolloMessageType";

        /// <summary>
        /// The 'id' field of an apollo message.
        /// </summary>
        public const string MESSAGE_ID = "apolloMessageId";

        /// <summary>
        /// The full route to the field that identifies the start of the subscription.
        /// </summary>
        public const string SUBSCRIPTION_ROUTE = "apolloSubscriptionRoute";

        /// <summary>
        /// The client supplied identifer assigned to the subscription.
        /// </summary>
        public const string SUBSCRIPTION_ID = "apolloSubscriptionId";

        /// <summary>
        /// The formal name of a given subscription event type.
        /// </summary>
        public const string SUBSCRIPTION_EVENT_NAME = "apolloSubscriptionEventName";

        /// <summary>
        /// The total number of clients being reported on.
        /// </summary>
        public const string CLIENT_COUNT = "clientCount";

        /// <summary>
        /// The total number of subscriptions being reported on.
        /// </summary>
        public const string SUBSCRIPTION_COUNT = "subscriptionCount";

        /// <summary>
        /// A collection of client ids being reported on.
        /// </summary>
        public const string CLIENT_IDS = "clientIds";

        /// <summary>
        /// A collection of subscription ids being reported on.
        /// </summary>
        public const string SUBSCRIPTION_IDS = "subscriptionIds";
    }
}