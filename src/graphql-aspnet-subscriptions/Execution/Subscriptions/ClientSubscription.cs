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
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A client subscription containing the details of what event is being listened for.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the subscription exists for.</typeparam>
    public class ClientSubscription<TSchema> : ISubscription<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscription{TSchema}" /> class.
        /// </summary>
        /// <param name="clientProxy">The client proxy that will own this subscription.</param>
        /// <param name="originalQuerydata">The original querydata that generated this subscription.</param>
        /// <param name="queryPlan">The query plan.</param>
        /// <param name="selectedOperation">The selected operation from the query plan
        /// from which to generate the subscription.</param>
        /// <param name="subscriptionid">A unique id to assign to this subscription. A guid id
        /// will be generated if this value is not supplied.</param>
        public ClientSubscription(
            ISubscriptionClientProxy clientProxy,
            GraphQueryData originalQuerydata,
            IGraphQueryPlan queryPlan,
            IGraphFieldExecutableOperation selectedOperation,
            string subscriptionid = null)
        {
            this.Client = Validation.ThrowIfNullOrReturn(clientProxy, nameof(clientProxy));
            this.QueryData = Validation.ThrowIfNullOrReturn(originalQuerydata, nameof(originalQuerydata));
            this.QueryOperation = Validation.ThrowIfNullOrReturn(selectedOperation, nameof(selectedOperation));
            this.QueryPlan = Validation.ThrowIfNullOrReturn(queryPlan, nameof(queryPlan));
            this.Messages = this.QueryPlan?.Messages ?? new GraphMessageCollection();

            this.Id = string.IsNullOrWhiteSpace(subscriptionid)
                ? Guid.NewGuid().ToString("N")
                : subscriptionid.Trim();

            this.IsValid = false;

            // parsing the query plan will garuntee that if the document contains
            // a subscription that it contains only one operation and
            // that a top level field will be a subscription-ready field.
            //
            // However, ensure that the operation that will be executed
            // does in fact represent a subscription being harnesssed
            if (this.QueryOperation.OperationType != GraphCollection.Subscription)
            {
                this.Messages.Critical(
                        $"The chosen operation is not a subscription operation.",
                        Constants.ErrorCodes.BAD_REQUEST);
                return;
            }

            var currentContext = this.QueryOperation.FieldContexts[0];

            // find the first non-virtual field referenced, it should be a controller
            // its garunteed to exist via the document generation rule engine
            // but it could be deep, walk down the subscirption tree to find it
            while (currentContext?.Field != null)
            {
                // when pointing at a subscription field we're done
                if (!currentContext.Field.IsVirtual)
                {
                    this.Field = currentContext.Field as ISubscriptionGraphField;
                    break;
                }

                currentContext = currentContext?.ChildContexts.Count == 1 ? currentContext.ChildContexts?[0] : null;
            }

            // just in case it wasn't found...
            // this is theoretically not possible but just in case
            // the user swaps out some DI components incorrectly or by mistake...
            if (this.Field == null)
            {
                this.Messages.Add(
                  GraphMessageSeverity.Critical,
                  "An eventable field could not found in the subscription operation. Ensure you include a field declared " +
                  "as a subscription field.",
                  Constants.ErrorCodes.BAD_REQUEST);
            }

            this.IsValid = this.Messages.IsSucessful && this.QueryOperation != null && this.Field != null;
        }

        /// <summary>
        /// Gets a unique identifier for this subscription instance.
        /// </summary>
        /// <value>A unique id for this subscription.</value>
        public string Id { get; }

        /// <summary>
        /// Gets a reference to the the top-level graph field that has been subscribed to.
        /// </summary>
        /// <value>The field.</value>
        public ISubscriptionGraphField Field { get; }

        /// <summary>
        /// Gets the unique route within a schema this subscription is pointed at.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route => this.Field?.Route;

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
        /// Gets the messages.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the original query data object that generated this subscription.
        /// </summary>
        /// <value>The query data.</value>
        public GraphQueryData QueryData { get; }

        /// <summary>
        /// Gets the generated query plan that was preparsed and represents the original subscription request.
        /// </summary>
        /// <value>The query plan.</value>
        public IGraphQueryPlan QueryPlan { get; }

        /// <summary>
        /// Gets the selected query operation that will be executed when new data is recieved for
        /// this subscription.
        /// </summary>
        /// <value>The query operation.</value>
        public IGraphFieldExecutableOperation QueryOperation { get; }
    }
}