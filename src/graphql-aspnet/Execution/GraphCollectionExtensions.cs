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
    /// Extension methods for working with <see cref="GraphCollection"/> enumeration.
    /// </summary>
    internal static class GraphCollectionExtensions
    {
        /// <summary>
        /// Converts the given <see cref="GraphCollection"/> value to
        /// its equivilant <see cref="GraphOperationType"/> value, if one exists.
        /// </summary>
        /// <param name="collectionItem">The collection item to convert.</param>
        /// <returns>GraphOperationType.</returns>
        public static GraphOperationType ToGraphOperationType(this GraphCollection collectionItem)
        {
            switch (collectionItem)
            {
                case GraphCollection.Query:
                    return GraphOperationType.Query;

                case GraphCollection.Mutation:
                    return GraphOperationType.Mutation;

                case GraphCollection.Subscription:
                    return GraphOperationType.Subscription;

                default:
                    return GraphOperationType.Unknown;
            }
        }
    }
}