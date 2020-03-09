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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// A wrapper on <see cref="GraphQueryExecutionContext"/> to provide property access
    /// to the meta data items added to the <see cref="GraphQueryExecutionContext"/> during execution.
    /// </summary>
    public class SubcriptionExecutionContext : GraphQueryExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcriptionExecutionContext" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="request">The request to be processed through the query pipeline.</param>
        /// <param name="subscriptionId">The unique id to assign to the created subscription, when one is made.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        public SubcriptionExecutionContext(
            ISubscriptionClientProxy client,
            IGraphOperationRequest request,
            string subscriptionId,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
            : base(request, client?.ServiceProvider, client?.User, metrics, logger, items)
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
                return (this.QueryOperation != null && this.QueryOperation.OperationType == GraphCollection.Subscription)
                    || this.Items.ContainsKey(SubscriptionConstants.Execution.CREATED_SUBSCRIPTION);
            }
        }

        /// <summary>
        /// Gets or sets the subscription that was created during execution. May be null if a subscription
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

            set
            {
                this.Items[SubscriptionConstants.Execution.CREATED_SUBSCRIPTION] = value;
            }
        }

        /// <summary>
        /// Gets the subscription client that is making this request.
        /// </summary>
        /// <value>The client.</value>
        public ISubscriptionClientProxy Client
            {
            get
            {
                if (this.Items.ContainsKey(SubscriptionConstants.Execution.CLIENT))
                    return this.Items[SubscriptionConstants.Execution.CLIENT] as ISubscriptionClientProxy;

                return null;
            }

            private set
            {
                this.Items[SubscriptionConstants.Execution.CLIENT] = value;
            }
        }

        /// <summary>
        /// Gets the unique identifier provided by the client at the time this
        /// context was created.
        /// </summary>
        /// <value>The subscription identifier.</value>
        public string SubscriptionId
        {
            get
            {
                if (this.Items.ContainsKey(SubscriptionConstants.Execution.SUBSCRIPTION_ID))
                    return this.Items[SubscriptionConstants.Execution.SUBSCRIPTION_ID]?.ToString();

                return null;
            }

            private set
            {
                this.Items[SubscriptionConstants.Execution.SUBSCRIPTION_ID] = value;
            }
        }
    }
}