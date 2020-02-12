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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
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
        /// <param name="clientProxy">The client proxy that will own this subscription.</param>
        /// <param name="clientProvidedId">The identifier, provided by the client, that will be sent
        /// whenever a response to this subscription is sent.</param>
        /// <param name="queryPlan">The query plan.</param>
        /// <param name="operationName">Name of the subscription operation within the document to use.</param>
        public ClientSubscription(
            ISubscriptionClientProxy clientProxy,
            string clientProvidedId,
            IGraphQueryPlan queryPlan,
            string operationName = null)
        {
            this.Client = Validation.ThrowIfNullOrReturn(clientProxy, nameof(clientProxy));
            this.ClientProvidedId = Validation.ThrowIfNullWhiteSpaceOrReturn(clientProvidedId, nameof(clientProvidedId));
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
            // However, ensure that the operation that will be executed
            // does in fact represent a subscription being harnesssed
            _operation = _queryPlan?.RetrieveOperation(operationName);
            if (_operation == null)
            {
                this.Messages.Critical(
                        $"No operation found with the name '{operationName}'.",
                        Constants.ErrorCodes.BAD_REQUEST);
                return;
            }

            if (_operation.OperationType != GraphCollection.Subscription)
            {
                this.Messages.Critical(
                        $"The chosen operation is not a subscription operation.",
                        Constants.ErrorCodes.BAD_REQUEST);
                return;
            }

            // find the first non-virtual field referenced, it should be a controller
            // field and a subscription-based field

            var currentContext = _operation.FieldContexts[0];
            var fieldErrorRecorded = false;
            while (_field != null)
            {
                // when pointing at a subscription field we're done
                if (currentField is ISubscriptionGraphField)
                {
                    _field = currentField as ISubscriptionGraphField;
                    break;
                }

                // when not pointing at a subscription field
                // we must be pointing at a virtual field or error
                // this allows us to walk down a controller custom route path to find
                // our subscription field while preserving the user's expected graph structure
                if (!currentField.IsVirtual)
                {
                    this.Messages.Add(
                      GraphMessageSeverity.Critical,
                      $"The first non-virtual field found in the subscription operation is not a valid subscription field (Path: {currentField.Route.Path})",
                      Constants.ErrorCodes.BAD_REQUEST);
                    break;
                }

                // when looking at a controller level field (or an intermediary)
                // it must have child fields for us to continue searching
                if (!(currentField is IGraphFieldContainer fieldSet) || fieldSet.Fields.Count != 1)
                {
                    this.Messages.Add(
                      GraphMessageSeverity.Critical,
                      $"The virtual field, {currentField.Route.Path}, contains no children found in the subscription operation is not a valid subscription field (Path: {currentField.Route.Path})",
                      Constants.ErrorCodes.BAD_REQUEST);
                    break;
                    break;
                }

                // also, said field must have exactly 1 child
                // in order to keep the data in tact. Otherwise its possible
                // that we have two "top level subscription fields" (indicating two seperate events)
                // in cased in one subscription.
                if (fieldSet.Fields.Count != 1)
                {

                }
            }

            if (_field == null && !fieldErrorRecorded)
            {
                // theoretically not possible but just in case
                // the user swaps out some DI components incorrectly or by mistake...
                this.Messages.Add(
                  GraphMessageSeverity.Critical,
                  $"An eventable field could not found in the subscription operation. Ensure you include a field declared " +
                  $"as a subscription field.",
                  Constants.ErrorCodes.BAD_REQUEST);
            }

            this.IsValid = this.Messages.IsSucessful && _operation != null && _field != null;
        }

        /// <summary>
        /// Gets a reference to the the top-level graph field that has been subscribed to.
        /// </summary>
        /// <value>The field.</value>
        public ISubscriptionGraphField Field => _field;

        /// <summary>
        /// Gets the unique route within a schema this subscription is pointed at.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route => _field?.Route;

        /// <summary>
        /// Gets a value indicating whether this subscription has been properly configured.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the client proxy that owns this subscription.
        /// </summary>
        /// <value>The client.</value>
        public ISubscriptionClientProxy Client { get; }

        /// <summary>
        /// Gets the client provided identifier that should be sent whenever data is sent for this subscription.
        /// </summary>
        /// <value>The client provided identifier.</value>
        public string ClientProvidedId { get; }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }
    }
}