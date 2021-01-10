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
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A dictionary wrapper to associate <see cref="SubscriptionEventName"/> to a
    /// unique set of <see cref="ISubscriptionEventReceiver"/> that will receive the event.
    /// </summary>
    public class SubscribedEventRecievers : Dictionary<SubscriptionEventName, HashSet<ISubscriptionEventReceiver>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribedEventRecievers"/> class.
        /// </summary>
        public SubscribedEventRecievers()
            : this(SubscriptionEventNameEqualityComparer.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribedEventRecievers"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see> for the type of the key.</param>
        public SubscribedEventRecievers(IEqualityComparer<SubscriptionEventName> comparer)
            : base(comparer)
        {
        }
    }
}