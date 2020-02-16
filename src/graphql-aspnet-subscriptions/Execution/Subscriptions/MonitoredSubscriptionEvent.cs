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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A qualified event being monitored by a listener.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to monitor for.</typeparam>
    public class MonitoredSubscriptionEvent<TSchema> : IMonitoredSubscriptionEvent
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoredSubscriptionEvent{TSchema}" /> class.
        /// </summary>
        /// <param name="field">The field being monitored.</param>
        public MonitoredSubscriptionEvent(ISubscriptionGraphField field)
        {
            this.Route = Validation.ThrowIfNullOrReturn(field, nameof(field)).Route;
            this.EventName = field.EventName?.Trim();
            if (string.IsNullOrWhiteSpace(this.EventName))
                this.EventName = null;
        }

        /// <summary>
        /// Gets the type of the schema being monitored for.
        /// </summary>
        /// <value>The type of the schema.</value>
        public Type SchemaType => typeof(TSchema);

        /// <summary>
        /// Gets an alternate short name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }

        /// <summary>
        /// Gets the fully qualified route defining the event.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; }
    }
}