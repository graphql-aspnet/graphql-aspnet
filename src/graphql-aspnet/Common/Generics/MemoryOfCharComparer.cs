// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An equality comparer to provide "string line" comparrisons for read only memory segments.
    /// </summary>
    public class MemoryOfCharComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
        private readonly StringComparison _comparer;

        /// <summary>
        /// Gets a singleton instance of the memory comparer using a case sensitive "Oridinal" comparer for
        /// individual character comparrison.
        /// </summary>
        /// <value>The default comparer.</value>
        public static MemoryOfCharComparer Instance { get; } = new MemoryOfCharComparer();

        /// <summary>
        /// Initializes static members of the <see cref="MemoryOfCharComparer"/> class.
        /// </summary>
        static MemoryOfCharComparer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryOfCharComparer"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public MemoryOfCharComparer(StringComparison comparer = StringComparison.Ordinal)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
        {
            if (x.Equals(y))
                return true;

            return x.Span.Equals(y.Span, _comparer);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(ReadOnlyMemory<char> obj)
        {
            return CombineHashCodes(obj.ToArray());
        }

        /// <summary>
        /// Combines the hash codes of the array together.
        /// </summary>
        /// <param name="hashCodes">The hash codes.</param>
        /// <returns>System.Int32.</returns>
        private static int CombineHashCodes(IEnumerable<char> hashCodes)
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            var i = 0;
            foreach (var hashCode in hashCodes)
            {
                if (i % 2 == 0)
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
                else
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

                ++i;
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}