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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Compares two <see cref="IGraphType" /> for equality.
    /// </summary>
    public sealed class GraphTypeEqualityComparer : IEqualityComparer<IGraphType>
    {
        /// <summary>
        /// Gets the singleton instance of this comparer.
        /// </summary>
        /// <value>The instance.</value>
        public static IEqualityComparer<IGraphType> Instance { get; } = new GraphTypeEqualityComparer();

        /// <summary>
        /// Prevents a default instance of the <see cref="GraphTypeEqualityComparer"/> class from being created.
        /// </summary>
        private GraphTypeEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(IGraphType x, IGraphType y)
        {
            return x?.Name == y?.Name;
        }

        /// <inheritdoc />
        public int GetHashCode(IGraphType obj)
        {
            return obj?.Name?.GetHashCode() ?? 0;
        }
    }
}