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
    using System.Diagnostics;
    using GraphQL.AspNet.Controllers;

    /// <summary>
    /// An event, raised by a <see cref="GraphController" /> and routed to various
    /// clients that are listening for new data.
    /// </summary>
    [DebuggerDisplay("Subscription Event: {EventName}")]
    public class SubscriptionEvent
    {
        /// <summary>
        /// Gets or sets the unique identifier assigned to this event when it was first raised
        /// from the source mutation or query.
        /// </summary>
        /// <value>The unique id of this event.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the data package that was supplied by the publisher when the event was raised.
        /// </summary>
        /// <value>The data object supplied when the event was published.</value>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified <see cref="Type"/> name of the <see cref="Data"/> object.
        /// </summary>
        /// <value>The data object's data type.</value>
        public string DataTypeName { get; set; }

        /// <summary>
        /// Gets or sets the schema-unique name of the event that was raised from the graphql server.
        /// </summary>
        /// <value>The event's unique name.</value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified <see cref="Type"/> name of the schema to which this
        /// event is targeted.
        /// </summary>
        /// <value>The target schema's data type.</value>
        public string SchemaTypeName { get; set; }

        /// <summary>
        /// Converts this instance to a fully qualified <see cref="SubscriptionEventName"/> object.
        /// </summary>
        /// <returns>An object useful for sorting and hashing events.</returns>
        public SubscriptionEventName ToSubscriptionEventName()
        {
            return new SubscriptionEventName(this.SchemaTypeName, this.EventName);
        }
    }
}