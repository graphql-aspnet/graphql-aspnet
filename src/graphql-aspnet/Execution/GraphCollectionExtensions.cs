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
    /// Extension methods for working with <see cref="SchemaItemCollections"/> enumeration.
    /// </summary>
    internal static class GraphCollectionExtensions
    {
        /// <summary>
        /// Converts the given <see cref="SchemaItemCollections"/> value to
        /// its equivilant <see cref="GraphOperationType"/> value, if one exists.
        /// </summary>
        /// <param name="collectionItem">The collection item to convert.</param>
        /// <returns>GraphOperationType.</returns>
        public static GraphOperationType ToGraphOperationType(this SchemaItemCollections collectionItem)
        {
            switch (collectionItem)
            {
                case SchemaItemCollections.Query:
                    return GraphOperationType.Query;

                case SchemaItemCollections.Mutation:
                    return GraphOperationType.Mutation;

                case SchemaItemCollections.Subscription:
                    return GraphOperationType.Subscription;

                default:
                    return GraphOperationType.Unknown;
            }
        }
    }
}