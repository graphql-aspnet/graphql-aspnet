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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions;

    /// <summary>
    /// Extension methods to expose subscription to graph controllers.
    /// </summary>
    public static class GraphQLControllerExtensions
    {
        /// <summary>
        /// Publishes the supplied data to the well-known internal event name, informing
        /// all listening subscriptions of the change. If the <paramref name="dataObject"/> is null the event
        /// is automatically canceled.
        /// </summary>
        /// <param name="controller">The controller from where the event is originating.</param>
        /// <param name="eventName">Name of the well-known event to be raised.</param>
        /// <param name="dataObject">The data object to pass with the event.</param>
        public static void PublishSubscriptionEvent(this GraphController controller, string eventName, object dataObject)
        {
            if (dataObject == null)
                return;

            eventName = Validation.ThrowIfNullWhiteSpaceOrReturn(eventName, nameof(eventName));

            var itemsCollection = controller.Request.Items;

            // add or reference the list of events
            if (!itemsCollection.TryGetValue(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, out var listObject))
            {
                listObject = new List<SubscriptionEventProxy>();
                itemsCollection.TryAdd(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, listObject);
            }

            var eventList = listObject as IList<SubscriptionEventProxy>;
            if (eventList == null)
            {
                throw new GraphExecutionException(
                    $"Unable to cast the context item '{SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY}' " +
                    $"(type: {listObject?.GetType().FriendlyName() ?? "unknown"}), into " +
                    $"{typeof(IList<SubscriptionEvent>).FriendlyName()}. Event '{eventName}' could not be published.",
                    controller.Request.Origin);
            }

            lock (eventList)
            {
                eventList.Add(new SubscriptionEventProxy(eventName, dataObject));
            }
        }
    }
}