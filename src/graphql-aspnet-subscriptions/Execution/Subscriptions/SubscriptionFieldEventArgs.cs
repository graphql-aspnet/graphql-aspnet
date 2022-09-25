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
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;

    /// <summary>
    /// The arguments related to the set of events that publish fields involved with subscription
    /// management.
    /// </summary>
    public class SubscriptionFieldEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionFieldEventArgs" /> class.
        /// </summary>
        /// <param name="graphField">The graph field.</param>
        public SubscriptionFieldEventArgs(ISubscriptionGraphField graphField)
        {
            this.Field = Validation.ThrowIfNullOrReturn(graphField, nameof(graphField));
        }

        /// <summary>
        /// Gets the field path route in context for this event.
        /// </summary>
        /// <value>The route.</value>
        public ISubscriptionGraphField Field { get; }
    }
}