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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of event arguments related to a <see cref="SubscriptionEvent"/> having an action taken against it.
    /// </summary>
    public class SubscriptionEventEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventEventArgs"/> class.
        /// </summary>
        /// <param name="subEvent">The sub event.</param>
        public SubscriptionEventEventArgs(SubscriptionEvent subEvent)
        {
        }
    }
}