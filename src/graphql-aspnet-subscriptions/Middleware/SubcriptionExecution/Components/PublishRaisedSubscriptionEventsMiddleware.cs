// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubcriptionExecution.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// Standard middleware component that pulls any raised events off the field execution context
    /// and pushes them to an internal queue for publishing.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component exists for.</typeparam>
    public class PublishRaisedSubscriptionEventsMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly SubscriptionEventQueue _eventQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishRaisedSubscriptionEventsMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="eventQueue">The event queue to add outgoing events to.</param>
        public PublishRaisedSubscriptionEventsMiddleware(SubscriptionEventQueue eventQueue)
        {
            _eventQueue = Validation.ThrowIfNullOrReturn(eventQueue, nameof(eventQueue));
        }

        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public Task InvokeAsync(
            GraphQueryExecutionContext context,
            GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next,
            CancellationToken cancelToken)
        {
            if (context?.Items != null && context.IsValid && !context.IsCancelled)
            {
                // if a context item for the subscription event key was added by one of the extension methods
                // inspect it to try and find the events that were registered
                if (context.Items.ContainsKey(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY))
                {
                    var collection = context.Items[SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY] as IList<SubscriptionEventProxy>;

                    if (collection == null)
                    {
                        throw new GraphExecutionException(
                            $"Unable to cast the context item '{SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY}' into " +
                            $"{typeof(IList<SubscriptionEventProxy>).FriendlyName()}. Published subscription events could not be raised.",
                            SourceOrigin.None);
                    }
                    else
                    {
                        foreach (var proxy in collection)
                        {
                            var eventData = new SubscriptionEvent()
                            {
                                Id = Guid.NewGuid().ToString(),
                                SchemaTypeName = SchemaExtensions.RetrieveFullyQualifiedSchemaTypeName(typeof(TSchema)),
                                Data = proxy.DataObject,
                                DataTypeName = SchemaExtensions.RetrieveFullyQualifiedDataObjectTypeName(proxy.DataObject?.GetType()),
                                EventName = proxy.EventName?.Trim(),
                            };

                            _eventQueue.Enqueue(eventData);
                        }
                    }
                }
            }

            return next(context, cancelToken);
        }
    }
}