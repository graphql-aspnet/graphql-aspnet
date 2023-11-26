// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults
{
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A helper class to allow the use of common <see cref="IGraphActionResult"/> methods
    /// with non-controller based resolvers for subscription related results.
    /// </summary>
    public static class SubscriptionGraphActionResult
    {
        /// <summary>
        /// When used as an action result from subscription, indicates that the subscription should be skipped
        /// and the connected client should receive NO data, as if the event never occured.
        /// </summary>
        /// <param name="completeSubscirption">if set to <c>true</c>, instructs that the
        /// subscription should also be gracefully end such that no additional events
        /// are processed after the event is skipped. The client may be informed of this operation if
        /// supported by its negotiated protocol.</param>
        /// <remarks>
        /// If used as an action result for a non-subscription action (i.e. a query or mutation) a critical
        /// error will be added to the response and the query will end.
        /// </remarks>
        /// <returns>An action result indicating that all field resolution results should be skipped
        /// and no data should be sent to the connected client.</returns>
        public static IGraphActionResult SkipSubscriptionEvent(bool completeSubscirption = false)
        {
            return new SkipSubscriptionEventGraphActionResult(completeSubscirption);
        }

        /// <summary>
        /// When used as an action result from subscription, resolves the field with the given object
        /// and indicates that to the client that the subscription should gracefully end when this event completes.
        /// Once completed, the subscription will be unregsitered and no additional events will
        /// be raised to this client. The client will be informed of this operation if supported
        /// by its negotiated protocol.
        /// </summary>
        /// <param name="item">The object to resolve the field with.</param>
        /// <remarks>
        /// If used as an action result for a non-subscription action (i.e. a query or mutation) a critical
        /// error will be added to the response and the query will end.
        /// </remarks>
        /// <returns>An action result indicating a successful field resolution with the supplied <paramref name="item"/>
        /// and additional information to instruct the subscription server to close the subscription
        /// once processing is completed.</returns>
        public static IGraphActionResult OkAndComplete(object item = null)
        {
            return new CompleteSubscriptionGraphActionResult(item);
        }
    }
}