// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubcriptionExecution
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A wrapper on <see cref="GraphQueryExecutionContext"/> to provide property access
    /// to the meta data items added to the <see cref="GraphQueryExecutionContext"/> during execution.
    /// </summary>
    public class SubcriptionExecutionContext : GraphQueryExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcriptionExecutionContext" /> class.
        /// </summary>
        /// <param name="request">The request to be processed through the query pipeline.</param>
        /// <param name="client">The client.</param>
        /// <param name="serviceProvider">The service provider used for the creation of any
        /// necessary objects during the execution of this query context.</param>
        /// <param name="querySession">The query session governing the execution of a query.</param>
        /// <param name="securityContext">The security context through which all
        /// field authorization processes will occur during this query.</param>
        /// <param name="subscriptionId">The unique id to assign to the created subscription, when one is made.</param>
        /// <param name="items">A collection of developer-driven items for tracking various pieces of data.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        public SubcriptionExecutionContext(
            IGraphOperationRequest request,
            ISubscriptionClientProxy client,
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            IUserSecurityContext securityContext,
            string subscriptionId,
            MetaDataCollection items = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null)
            : base(request, serviceProvider, querySession, items, securityContext, metrics, logger)
        {
            this.Client = Validation.ThrowIfNullOrReturn(client, nameof(client));
            this.SubscriptionId = Validation.ThrowIfNullWhiteSpaceOrReturn(subscriptionId, nameof(subscriptionId));
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
                return (this.QueryPlan?.Operation != null && this.QueryPlan.Operation.OperationType == GraphOperationType.Subscription)
                    || this.Subscription != null;
            }
        }

        /// <summary>
        /// Gets or sets the subscription that was created during execution. May be null if a subscription
        /// was not successfully created. Inspect <see cref="IsSubscriptionOperation"/> to determine if
        /// a subscription should have been created but failed.
        /// </summary>
        /// <value>The subscription.</value>
        public ISubscription Subscription { get; set; }

        /// <summary>
        /// Gets the subscription client that is making this request.
        /// </summary>
        /// <value>The client.</value>
        public ISubscriptionClientProxy Client { get; private set; }

        /// <summary>
        /// Gets the unique identifier provided by the client at the time this
        /// context was created.
        /// </summary>
        /// <value>The subscription identifier.</value>
        public string SubscriptionId { get; private set; }
    }
}