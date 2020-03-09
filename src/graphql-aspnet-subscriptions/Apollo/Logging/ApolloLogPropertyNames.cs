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
    }
}