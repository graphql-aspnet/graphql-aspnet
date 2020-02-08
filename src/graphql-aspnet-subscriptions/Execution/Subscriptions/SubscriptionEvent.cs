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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// An event, raised by a <see cref="GraphController" />, and handled by a <see cref="ISubscriptionServer{TSchema}" />
    /// to transfer the given data package to connected clients.
    /// </summary>
    /// <typeparam name="TData">The type of the data the event is transmitting.</typeparam>
    public class SubscriptionEvent<TData>
    {
        /// <summary>
        /// Gets or sets the data package that was supplied by the publisher when the event was raised.
        /// </summary>
        /// <value>The data.</value>
        public TData Data { get; set; }

        /// <summary>
        /// Gets or sets the schema-unique name of the event that was raised.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the full field path representing the source method where this event was raised.
        /// </summary>
        /// <value>The source route.</value>
        public GraphFieldPath SourceRoute { get; set; }

        /// <summary>
        /// Gets or sets the globally unique schema id from which this event
        /// was published.
        /// </summary>
        /// <value>The schema identifier.</value>
        public string SchemaId { get; set; }
    }
}