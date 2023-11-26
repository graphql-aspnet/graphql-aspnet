﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System.Collections.Generic;

    /// <summary>
    /// An equality comparer for <see cref="ItemPath"/> objects.
    /// </summary>
    public class ItemPathComparer : IEqualityComparer<ItemPath>
    {
        /// <summary>
        /// Gets a singleton instance of the comparer declared with default parameters.
        /// </summary>
        /// <value>The instance.</value>
        public static ItemPathComparer Instance { get; }

        /// <summary>
        /// Initializes static members of the <see cref="ItemPathComparer"/> class.
        /// </summary>
        static ItemPathComparer()
        {
            ItemPathComparer.Instance = new ItemPathComparer();
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(ItemPath x, ItemPath y)
        {
            return x == y;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(ItemPath obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}