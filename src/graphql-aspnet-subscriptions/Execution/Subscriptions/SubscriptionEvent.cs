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
        public string DataTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the event that was raised from the graphql server.
        /// </summary>
        /// <value>The source route.</value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified type name of the schema to which this
        /// event is targeted.
        /// </summary>
        /// <value>The schema identifier.</value>
        public string SchemaTypeName { get; set; }

        /// <summary>
        /// Converts this instance to a fully qualified <see cref="SubscriptionEventName"/> object.
        /// </summary>
        /// <returns>SubscriptionEventName.</returns>
        public SubscriptionEventName ToSubscriptionEventName()
        {
            return new SubscriptionEventName(this.SchemaTypeName, this.EventName);
        }
    }
}