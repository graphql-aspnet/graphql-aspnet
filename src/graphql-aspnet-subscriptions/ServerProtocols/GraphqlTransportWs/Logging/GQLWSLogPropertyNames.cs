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
    /// <summary>
    /// A set of known property names for graphql-ws related log entries.
    /// </summary>
    public static class GqltwsLogPropertyNames
    {
        /// <summary>
        /// The 'type' field of an graphql-ws message.
        /// </summary>
        public const string MESSAGE_TYPE = "graphqlwsMessageType";

        /// <summary>
        /// The 'id' field of an graphql-ws message.
        /// </summary>
        public const string MESSAGE_ID = "graphqlwsMessageId";

        /// <summary>
        /// The full route to the field that identifies the start of the subscription.
        /// </summary>
        public const string SUBSCRIPTION_ROUTE = "graphqlwsSubscriptionRoute";

        /// <summary>
        /// The client supplied identifer assigned to the subscription.
        /// </summary>
        public const string SUBSCRIPTION_ID = "graphqlwsSubscriptionId";

        /// <summary>
        /// The formal name of a given subscription event type.
        /// </summary>
        public const string SUBSCRIPTION_EVENT_NAME = "graphqlwsSubscriptionEventName";

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