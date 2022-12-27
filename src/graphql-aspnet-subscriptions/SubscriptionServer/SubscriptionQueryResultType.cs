// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    /// <summary>
    /// A status indicating the completion of a subscription enabled query.
    /// </summary>
    public enum SubscriptionQueryResultType
    {
        None = 0,

        /// <summary>
        /// The identifer of the subscription request is already in use by a registered
        /// subscription.
        /// </summary>
        IdInUse = 1,

        /// <summary>
        /// The subscription was successfully registered.
        /// </summary>
        SubscriptionRegistered = 2,

        /// <summary>
        /// The request was a single query (not a subscription and it completed successfully).
        /// </summary>
        SingleQueryCompleted = 3,

        /// <summary>
        /// Execution of the request failed to complete correctly.
        /// </summary>
        OperationFailure = 4,
    }
}