// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A client subscription containing the details of what event is being listened for.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the subscription exists for.</typeparam>
    public class ClientSubscription<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IGraphQueryPlan _queryPlan;
        private readonly IGraphFieldExecutableOperation _operation;
        private IGraphSubscriptionField _field;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscription{TSchema}" /> class.
        /// </summary>
        /// <param name="queryPlan">The query plan.</param>
        public ClientSubscription(IGraphQueryPlan queryPlan)
        {
            this.Messages = _queryPlan?.Messages ?? new GraphMessageCollection();
            _queryPlan = queryPlan;

            this.IsValid = false;
            if (_queryPlan != null)
            {
                this.Messages.AddRange(_queryPlan.Messages);
            }
            else
            {
                this.Messages.Add(
                    GraphMessageSeverity.Critical,
                    "No query plan was generated for the requested subscription.",
                    Constants.ErrorCodes.BAD_REQUEST);
                return;
            }

            // parsing the query plan will garuntee that if the document contains
            // a subscription that it contains only one operation and
            // that a top level field will be a subscription-ready field.
            //
            // However, ensure that the query docuemnt does in fact represent a subscription
            // operation
            _operation = _queryPlan?.RetrieveOperation(null);
            if (_operation != null
                && _operation.OperationType == GraphCollection.Subscription
                && _operation.FieldContexts.Count == 1)
            {
                _field = _operation.FieldContexts[0]?.Field as IGraphSubscriptionField;
                if (_field == null)
                {
                    this.Messages.Add(
                      GraphMessageSeverity.Critical,
                      $"The subscription query's top level field, '{_operation.FieldContexts[0]?.Field?.Name}', is not a valid subscription field.",
                      Constants.ErrorCodes.BAD_REQUEST);
                }
            }

            this.IsValid = this.Messages.IsSucessful && _operation != null && _field != null;
            this.EventName = _field?.EventName;
        }

        /// <summary>
        /// Gets the name of the schema-unique event this subscription is waiting to hear from.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }

        /// <summary>
        /// Gets a value indicating whether this subscription has been properly configured.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }
    }
}