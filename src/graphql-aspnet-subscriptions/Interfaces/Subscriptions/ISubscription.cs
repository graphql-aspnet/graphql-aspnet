// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A client subscription containing the details of what event is being listened for.
    /// </summary>
    public interface ISubscription
    {
        /// <summary>
        /// Gets a unique identifier for this subscription instance.
        /// </summary>
        /// <value>A unique id for this subscription.</value>
        string Id { get; }

        /// <summary>
        /// Gets a reference to the the top-level graph field that has been subscribed to.
        /// </summary>
        /// <value>The field.</value>
        ISubscriptionGraphField Field { get; }

        /// <summary>
        /// Gets the unique route within a schema this subscription is pointed at.
        /// </summary>
        /// <value>The route.</value>
        GraphFieldPath Route { get; }

        /// <summary>
        /// Gets the original query data object that generated this subscription.
        /// </summary>
        /// <value>The query data.</value>
        GraphQueryData QueryData { get; }

        /// <summary>
        /// Gets the generated query plan that was preparsed and represents the original subscription request.
        /// </summary>
        /// <value>The query plan.</value>
        IGraphQueryPlan QueryPlan { get; }

        /// <summary>
        /// Gets the selected query operation that will be executed when new data is recieved for
        /// this subscription.
        /// </summary>
        /// <value>The query operation.</value>
        IGraphFieldExecutableOperation QueryOperation { get; }

        /// <summary>
        /// Gets a value indicating whether this subscription has been properly configured.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets the client proxy that owns this subscription.
        /// </summary>
        /// <value>The client.</value>
        ISubscriptionClientProxy Client { get; }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }
    }
}