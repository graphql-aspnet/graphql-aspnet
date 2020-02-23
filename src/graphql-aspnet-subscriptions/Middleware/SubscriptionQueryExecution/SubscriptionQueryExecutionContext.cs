// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubscriptionQueryExecution
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// A wrapper on <see cref="GraphQueryExecutionContext"/> to provide property access
    /// to the meta data items added to the <see cref="GraphQueryExecutionContext"/> during execution.
    /// </summary>
    public class SubscriptionQueryExecutionContext : GraphQueryExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionQueryExecutionContext"/> class.
        /// </summary>
        /// <param name="request">The request to be processed through the query pipeline.</param>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="user">The user authenticated by the Asp.net runtime.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        public SubscriptionQueryExecutionContext(
            IGraphOperationRequest request,
            IServiceProvider serviceProvider,
            ClaimsPrincipal user = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
            : base(request, serviceProvider, user, metrics, logger, items)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the executed context represents one that did (or should have)
        /// generated a subscription.
        /// </summary>
        /// <value><c>true</c> if this instance executed a subscription operation; otherwise, <c>false</c>.</value>
        public bool IsSubscriptionOperation
        {
            get
            {
                return (this.QueryOperation != null && this.QueryOperation.OperationType == GraphCollection.Subscription)
                    || this.Items.ContainsKey(SubscriptionConstants.Execution.CREATED_SUBSCRIPTION);
            }
        }

        /// <summary>
        /// Gets the subscription that was created during execution. May be null if a subscription
        /// was not successfully created. Inspect <see cref="IsSubscriptionOperation"/> to determine if
        /// a subscription should have been created but failed.
        /// </summary>
        /// <value>The subscription.</value>
        public ISubscription Subscription
        {
            get
            {
                if (this.Items.ContainsKey(SubscriptionConstants.Execution.CREATED_SUBSCRIPTION))
                    return this.Items[SubscriptionConstants.Execution.CREATED_SUBSCRIPTION] as ISubscription;

                return null;
            }
        }
    }
}