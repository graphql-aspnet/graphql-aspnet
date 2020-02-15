﻿// *************************************************************
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

    /// <summary>
    /// An event, raised by a <see cref="GraphController" />, and handled by a <see cref="ISubscriptionServer{TSchema}" />
    /// to transfer the given data package to connected clients.
    /// </summary>
    public class SubscriptionEvent
    {
        /// <summary>
        /// Gets or sets the data package that was supplied by the publisher when the event was raised.
        /// </summary>
        /// <value>The data.</value>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified type name of the data object.
        /// </summary>
        /// <value>The type.</value>
        public string TypeName { get; set; }

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