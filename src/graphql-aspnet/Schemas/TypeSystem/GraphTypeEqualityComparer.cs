// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Compares two <see cref="IGraphType"/> for equality.
    /// </summary>
    public sealed class GraphTypeEqualityComparer : IEqualityComparer<IGraphType>
    {
        /// <summary>
        /// Gets the single instance of this comparer.
        /// </summary>
        /// <value>The instance.</value>
        public static GraphTypeEqualityComparer Instance { get; } = new GraphTypeEqualityComparer();

        /// <summary>
        /// Prevents a default instance of the <see cref="GraphTypeEqualityComparer" /> class from being created.
        /// </summary>
        private GraphTypeEqualityComparer()
        {
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(IGraphType x, IGraphType y)
        {
            return x?.Name == y?.Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(IGraphType obj)
        {
            return obj?.Name?.GetHashCode() ?? 0;
        }
    }
}