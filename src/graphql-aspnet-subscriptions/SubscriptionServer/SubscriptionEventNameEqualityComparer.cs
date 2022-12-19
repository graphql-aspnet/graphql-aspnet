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

    /// <summary>
    /// An equality comaprer for <see cref="SubscriptionEventName"/> objects.
    /// </summary>
    public class SubscriptionEventNameEqualityComparer : IEqualityComparer<SubscriptionEventName>
    {
        /// <summary>
        /// Gets the singleton instance of the comparer.
        /// </summary>
        /// <value>The instance.</value>
        public static IEqualityComparer<SubscriptionEventName> Instance { get; }

        /// <summary>
        /// Initializes static members of the <see cref="SubscriptionEventNameEqualityComparer"/> class.
        /// </summary>
        static SubscriptionEventNameEqualityComparer()
        {
            Instance = new SubscriptionEventNameEqualityComparer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventNameEqualityComparer"/> class.
        /// </summary>
        public SubscriptionEventNameEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(SubscriptionEventName x, SubscriptionEventName y)
        {
            return x.EventName == y.EventName
                && x.OwnerSchemaType == y.OwnerSchemaType;
        }

        /// <inheritdoc />
        public int GetHashCode(SubscriptionEventName obj)
        {
            return obj.GetHashCode();
        }
    }
}