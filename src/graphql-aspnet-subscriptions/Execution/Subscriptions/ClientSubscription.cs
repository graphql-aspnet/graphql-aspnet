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
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A client subscription containing the details of what event is being listened for.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the subscription exists for.</typeparam>
    public class ClientSubscription<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IGraphQueryPlan _queryPlan;
        private readonly IGraphFieldExecutableOperation _operation;
        private ISubscriptionGraphField _field;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscription{TSchema}" /> class.
        /// </summary>
        /// <param name="queryPlan">The query plan.</param>
        /// <param name="operationName">Name of the subscription operation within the document to use.</param>
        public ClientSubscription(IGraphQueryPlan queryPlan, string operationName = null)
        {
            this.Messages = _queryPlan?.Messages ?? new GraphMessageCollection();
            _queryPlan = queryPlan;

            this.IsValid = false;
            if (_queryPlan != null)
            {
                this.Messages.AddRange(_queryPlan.Messages);
                if (!_queryPlan.IsValid)
                    return;
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
            // operation and that a field is defined
            _operation = _queryPlan?.RetrieveOperation(operationName);
            if (_operation != null
                && _operation.OperationType == GraphCollection.Subscription
                && _operation.FieldContexts.Count == 1)
            {
                _field = _operation.FieldContexts[0]?.Field as ISubscriptionGraphField;
                if (_field == null)
                {
                    this.Messages.Add(
                      GraphMessageSeverity.Critical,
                      $"The subscription query's top level field, '{_operation.FieldContexts[0]?.Field?.Name}', is not a valid subscription field.",
                      Constants.ErrorCodes.BAD_REQUEST);
                }
                else
                {
                    this.Route = _field.Route;
                }
            }

            this.IsValid = this.Messages.IsSucessful && _operation != null && _field != null;
        }

        /// <summary>
        /// Gets the unique route within a schema this subscription is pointed at.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; }

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