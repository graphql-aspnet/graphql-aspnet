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
    /// Extension methods for working with <see cref="SchemaItemPathCollections"/> enumeration.
    /// </summary>
    internal static class SchemaItemPathCollectionsExtensions
    {
        /// <summary>
        /// Converts the given <see cref="SchemaItemPathCollections"/> value to
        /// its equivilant <see cref="GraphOperationType"/> value, if one exists.
        /// </summary>
        /// <param name="collectionItem">The collection item to convert.</param>
        /// <returns>GraphOperationType.</returns>
        public static GraphOperationType ToGraphOperationType(this SchemaItemPathCollections collectionItem)
        {
            switch (collectionItem)
            {
                case SchemaItemPathCollections.Query:
                    return GraphOperationType.Query;

                case SchemaItemPathCollections.Mutation:
                    return GraphOperationType.Mutation;

                case SchemaItemPathCollections.Subscription:
                    return GraphOperationType.Subscription;

                default:
                    return GraphOperationType.Unknown;
            }
        }
    }
}