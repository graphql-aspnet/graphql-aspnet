// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.Common
{
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A result encapsulating the execution of graph query that could be a subscription.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema executed against.</typeparam>
    public class SubscriptionDataExecutionResult<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates a result indicating that the attempted <paramref name="subscriptionId"/>
        /// was already in use and could not be executed.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns>SubscriptionDataExecutionResult.</returns>
        public static SubscriptionDataExecutionResult<TSchema> DuplicateId(string subscriptionId)
        {
            var result = new SubscriptionDataExecutionResult<TSchema>();
            result.SubscriptionId = subscriptionId;

            var message = new GraphExecutionMessage(
                                GraphMessageSeverity.Critical,
                                $"The message id '{subscriptionId}' is already reserved for an outstanding request and cannot " +
                                "be processed against. Allow the in-progress request to complete or stop the associated subscription.",
                                SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID);

            var messages = new GraphMessageCollection();
            messages.Add(message);

            result.Messages = messages;
            result.Status = SubscriptionOperationResultType.IdInUse;

            return result;
        }

        /// <summary>
        /// Generates an execution result indicating that a subscription was successfully registered.
        /// </summary>
        /// <param name="subscription">The subscription that was registered.</param>
        /// <returns>SubscriptionDataExecutionResult&lt;TSchema&gt;.</returns>
        public static SubscriptionDataExecutionResult<TSchema> SubscriptionRegistered(ISubscription<TSchema> subscription)
        {
            var result = new SubscriptionDataExecutionResult<TSchema>();
            result.SubscriptionId = subscription.Id;
            result.Subscription = subscription;
            result.Status = SubscriptionOperationResultType.SubscriptionRegistered;
            result.Messages = subscription.Messages;
            return result;
        }

        /// <summary>
        /// Generates a result indicating that an operation that was to be executed failed to
        /// do so.
        /// </summary>
        /// <param name="subscriptionId">The subscription id that was executed.</param>
        /// <param name="messages">The messages that were generated during the failure.</param>
        /// <returns>SubscriptionDataExecutionResult&lt;TSchema&gt;.</returns>
        public static SubscriptionDataExecutionResult<TSchema> OperationFailure(
            string subscriptionId,
            IGraphMessageCollection messages)
        {
            var result = new SubscriptionDataExecutionResult<TSchema>();
            result.SubscriptionId = subscriptionId;

            result.Status = SubscriptionOperationResultType.OperationFailure;

            var messageSet = new GraphMessageCollection();
            messageSet.AddRange(messages);
            result.Messages = messageSet;

            return result;
        }

        /// <summary>
        /// Generates a result indicating that the subscription enabled query was
        /// successfully executed a single query and no subscription was created.
        /// </summary>
        /// <param name="subscriptionId">The subscription id that was executed.</param>
        /// <param name="operationResult">The completed query operation result.</param>
        /// <returns>SubscriptionDataExecutionResult&lt;TSchema&gt;.</returns>
        public static SubscriptionDataExecutionResult<TSchema> SingleOperationCompleted(string subscriptionId, IGraphOperationResult operationResult)
        {
            var result = new SubscriptionDataExecutionResult<TSchema>();
            result.SubscriptionId = subscriptionId;

            result.Status = SubscriptionOperationResultType.SingleQueryCompleted;

            result.OperationResult = operationResult;
            result.Messages = operationResult.Messages;

            return result;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SubscriptionDataExecutionResult{TSchema}"/> class from being created.
        /// </summary>
        private SubscriptionDataExecutionResult()
        {
        }

        /// <summary>
        /// Gets the original subscription id that was executed to produce this result.
        /// </summary>
        /// <value>The subscription identifier.</value>
        public string SubscriptionId { get; private set; }

        /// <summary>
        /// Gets the status indicated by this result.
        /// </summary>
        /// <value>The status.</value>
        public SubscriptionOperationResultType Status { get; private set; }

        /// <summary>
        /// Gets the operation result of a completed single operation query, if any. Will be
        /// null if the executed operation registered a <see cref="Subscription"/> instead.
        /// </summary>
        /// <value>The operation result.</value>
        public IGraphOperationResult OperationResult { get; private set; }

        /// <summary>
        /// Gets the subscription that was created if the executed query was a
        /// subscription based query that was completed. Will be null of the executed operation
        /// generated a <see cref="OperationResult"/> instead.
        /// </summary>
        /// <value>The subscription.</value>
        public ISubscription<TSchema> Subscription { get; private set; }

        /// <summary>
        /// Gets the collection of messages generated during execution that should be
        /// communicated to the user.
        /// </summary>
        /// <value>The generated execution messages.</value>
        public IGraphMessageCollection Messages { get; private set; }
    }
}