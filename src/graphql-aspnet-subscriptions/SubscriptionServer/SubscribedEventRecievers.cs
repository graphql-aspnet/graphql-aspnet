// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// A dictionary wrapper to associate <see cref="SubscriptionEventName"/> to a
    /// unique set of <see cref="SubscriptionClientId"/> representing connected clients
    /// that will receive the event.
    /// </summary>
    internal sealed class SubscribedEventRecievers : Dictionary<SubscriptionEventName, HashSet<SubscriptionClientId>>
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
        /// <param name="comparer">An equality comparer to compare sameness of subscription events.</param>
        public SubscribedEventRecievers(IEqualityComparer<SubscriptionEventName> comparer)
            : base(comparer)
        {
        }
    }
}