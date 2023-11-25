// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution
{
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for working with <see cref="ItemPathRoots"/> enumeration.
    /// </summary>
    internal static class ItemPathRootsExtensions
    {
        /// <summary>
        /// Converts the given <see cref="ItemPathRoots"/> value to
        /// its equivilant <see cref="GraphOperationType"/> value, if one exists.
        /// </summary>
        /// <param name="collectionItem">The collection item to convert.</param>
        /// <returns>GraphOperationType.</returns>
        public static GraphOperationType ToGraphOperationType(this ItemPathRoots collectionItem)
        {
            switch (collectionItem)
            {
                case ItemPathRoots.Query:
                    return GraphOperationType.Query;

                case ItemPathRoots.Mutation:
                    return GraphOperationType.Mutation;

                case ItemPathRoots.Subscription:
                    return GraphOperationType.Subscription;

                default:
                    return GraphOperationType.Unknown;
            }
        }
    }
}