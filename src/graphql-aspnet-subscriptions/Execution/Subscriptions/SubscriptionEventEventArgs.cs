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

    /// <summary>
    /// A set of event arguments related to a <see cref="SubscriptionEvent"/> having an action taken against it.
    /// </summary>
    public class SubscriptionEventEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventEventArgs" /> class.
        /// </summary>
        /// <param name="eventData">The event data being acted on.</param>
        public SubscriptionEventEventArgs(SubscriptionEvent eventData)
        {
            this.SubscriptionEvent = Validation.ThrowIfNullOrReturn(eventData, nameof(eventData));
        }

        /// <summary>
        /// Gets the subscription event being acted on.
        /// </summary>
        /// <value>The subscription event.</value>
        public SubscriptionEvent SubscriptionEvent { get; }
    }
}