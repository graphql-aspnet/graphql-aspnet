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

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(SubscriptionEventName x, SubscriptionEventName y)
        {
            return x == y;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(SubscriptionEventName obj)
        {
            return obj.GetHashCode();
        }
    }
}