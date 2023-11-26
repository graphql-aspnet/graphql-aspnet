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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.SubscriptionServer;

    /// <summary>
    /// A helper class to allow the use of common <see cref="IGraphActionResult"/> methods
    /// with non-controller based resolvers for subscription related results.
    /// </summary>
    public static class SubscriptionEvents
    {
        /// <summary>
        /// Publishes an instance of the internal event, informing all graphql-subscriptions that
        /// are subscribed to the event. If the <paramref name="dataObject" /> is
        /// <c>null</c> the event is automatically canceled.
        /// </summary>
        /// <param name="context">The resolution context of the field where the event is being published.</param>
        /// <param name="eventName">Name of the well-known event to be raised.</param>
        /// <param name="dataObject">The data object to pass with the event.</param>
        public static void PublishSubscriptionEvent(this SchemaItemResolutionContext context, string eventName, object dataObject)
        {
            Validation.ThrowIfNull(context, nameof(context));
            Validation.ThrowIfNull(dataObject, nameof(dataObject));
            eventName = Validation.ThrowIfNullWhiteSpaceOrReturn(eventName, nameof(eventName));

            var contextData = context.Session.Items;

            // add or reference the list of events on the active context
            var eventsCollectionFound = contextData
                .TryGetValue(
                    SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION,
                    out var listObject);

            if (!eventsCollectionFound)
            {
                listObject = new List<SubscriptionEventProxy>(1);
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
                    context.Request.Origin);
            }

            lock (eventList)
            {
                eventList.Add(new SubscriptionEventProxy(eventName, dataObject));
            }
        }
    }
}