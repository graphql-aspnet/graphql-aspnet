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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions;
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
            if (dataObject == null)
                return;

            var contextData = controller.Context.Session.Items;

            // add or reference the list of events
            var eventsCollectionFound = contextData
                .TryGetValue(
                    SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION,
                    out var listObject);

            if (!eventsCollectionFound)
            {
                listObject = new List<SubscriptionEventProxy>();
                contextData.TryAdd(
                    SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION,
                    listObject);
            }

            var eventList = listObject as IList<SubscriptionEventProxy>;
            if (eventList == null)
            {
                throw new GraphExecutionException(
                    $"Unable to cast the context data item '{SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION}' " +
                    $"(type: {listObject?.GetType().FriendlyName() ?? "unknown"}), into " +
                    $"{typeof(IList<SubscriptionEvent>).FriendlyName()}. Event '{eventName}' could not be published.",
                    controller.Request.Origin);
            }

            eventName = Validation.ThrowIfNullWhiteSpaceOrReturn(eventName, nameof(eventName));

            lock (eventList)
            {
                eventList.Add(new SubscriptionEventProxy(eventName, dataObject));
            }
        }

        /// <summary>
        /// When called from a subscription, indicates that the subscription should be skipped
        /// and the connected client should receive NO data, as if the event enver occured.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> Issues a bad request and terminates the query for non-subscription action methods.
        /// </remarks>
        /// <param name="controller">The controller that contains the subscription method.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult SkipSubscriptionEvent(this GraphController controller)
        {
            return new SkipSubscriptionEventActionResult();
        }

        /// <summary>
        /// When called from a subscription, resolves the field with a <c>null</c> result and indicates that the subscription should end this event completes.
        /// Once completed, the subscription will be unregsitered and no additional events will
        /// be raised to this client.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult Complete(this GraphController controller)
        {
            return Complete(controller, null);
        }

        /// <summary>
         /// When called from a subscription, resolves the field with the given object
         /// and indicates that to the client that the subscription should end when this event completes.
         /// Once completed, the subscription will be unregsitered and no additional events will
         /// be raised to this client.
         /// </summary>
         /// <param name="controller">The controller.</param>
         /// <param name="item">The object to resolve the field with.</param>
         /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult Complete(this GraphController controller, object item)
        {
            return new CompleteSubscriptionActionResult(item);
        }
    }
}