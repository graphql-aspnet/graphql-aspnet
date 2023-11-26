// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers
{
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// Extension methods to expose subscription to graph controllers.
    /// </summary>
    public static class GraphControllerExtensions
    {
        /// <summary>
        /// Publishes an instance of the internal event, informing all graphql-subscriptions that
        /// are subscribed to the event. If the <paramref name="dataObject"/> is
        /// <c>null</c> the event is automatically canceled.
        /// </summary>
        /// <param name="controller">The controller from where the event is originating.</param>
        /// <param name="eventName">Name of the well-known event to be raised.</param>
        /// <param name="dataObject">The data object to pass with the event.</param>
        public static void PublishSubscriptionEvent(this GraphController controller, string eventName, object dataObject)
        {
            controller?.Context?.PublishSubscriptionEvent(eventName, dataObject);
        }

        /// <summary>
        /// When used as an action result from subscription, indicates that the subscription should be skipped
        /// and the connected client should receive NO data, as if the event never occured.
        /// </summary>
        /// <param name="controller">The controller that contains the subscription method.</param>
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
        public static IGraphActionResult SkipSubscriptionEvent(this GraphController controller, bool completeSubscirption = false)
        {
            return SubscriptionGraphActionResult.SkipSubscriptionEvent(completeSubscirption);
        }

        /// <summary>
        /// When used as an action result from subscription, resolves the field with the given object
        /// and indicates that to the client that the subscription should gracefully end when this event completes.
        /// Once completed, the subscription will be unregsitered and no additional events will
        /// be raised to this client. The client will be informed of this operation if supported
        /// by its negotiated protocol.
        /// </summary>
        /// <param name="controller">The controller that contains the subscription method.</param>
        /// <param name="item">The object to resolve the field with.</param>
        /// <remarks>
        /// If used as an action result for a non-subscription action (i.e. a query or mutation) a critical
        /// error will be added to the response and the query will end.
        /// </remarks>
        /// <returns>An action result indicating a successful field resolution with the supplied <paramref name="item"/>
        /// and additional information to instruct the subscription server to close the subscription
        /// once processing is completed.</returns>
        public static IGraphActionResult OkAndComplete(this GraphController controller, object item = null)
        {
            return SubscriptionGraphActionResult.OkAndComplete(item);
        }
    }
}