// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubscriptionEventExecution
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// An execution context carried throughout the subscription execution pipeline to
    /// complete processing of one event received by this subscription server for one registered
    /// subscription.
    /// </summary>
    public class GraphSubscriptionExecutionContext : BaseGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSubscriptionExecutionContext"/> class.
        /// </summary>
        /// <param name="eventData">The event data being processed.</param>
        /// <param name="subscription">The subscription this context is acting against.</param>
        /// <param name="otherContext">The other context to extract data from.</param>
        public GraphSubscriptionExecutionContext(
            SubscriptionEvent eventData,
            ISubscription subscription,
            IGraphMiddlewareContext otherContext)
            : base(otherContext)
        {
            this.SubscriptionEvent = Validation.ThrowIfNullOrReturn(eventData, nameof(eventData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSubscriptionExecutionContext" /> class.
        /// </summary>
        /// <param name="eventData">The event data being processed.</param>
        /// <param name="subscription">The subscription this context is acting against.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        public GraphSubscriptionExecutionContext(
            SubscriptionEvent eventData,
            ISubscription subscription,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
            : base(subscription?.Client?.ServiceProvider, subscription?.Client?.User, metrics, logger, items)
        {
            this.SubscriptionEvent = Validation.ThrowIfNullOrReturn(eventData, nameof(eventData));
            this.Subscription = Validation.ThrowIfNullOrReturn(subscription, nameof(subscription));
        }

        /// <summary>
        /// Gets the subscription event that was raised to the server instance and should
        /// be processed against the client.
        /// </summary>
        /// <value>The subscription event.</value>
        public SubscriptionEvent SubscriptionEvent { get; }

        /// <summary>
        /// Gets the single registered subscription for which the pipeleine is being executed.
        /// </summary>
        /// <value>The subscription.</value>
        public ISubscription Subscription { get; }
    }
}