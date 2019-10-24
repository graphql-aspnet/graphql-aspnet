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
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// Helper methods for <see cref="GraphCollection"/>.
    /// </summary>
    public static class GraphCollectionExtensionMethods
    {
        /// <summary>
        /// Converts the value into its equivilant routing constant.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public static string ToRouteRoot(this GraphCollection value)
        {
            switch (value)
            {
                case GraphCollection.Query:
                    return RouteConstants.QUERY_ROOT;

                case GraphCollection.Mutation:
                    return RouteConstants.MUTATION_ROOT;

                case GraphCollection.Subscription:
                    return RouteConstants.SUBSCRIPTION_ROOT;

                case GraphCollection.Types:
                    return RouteConstants.TYPE_ROOT;

                case GraphCollection.Enums:
                    return RouteConstants.ENUM_ROOT;

                case GraphCollection.Directives:
                    return RouteConstants.DIRECTIVE_ROOT;

                default:
                    return RouteConstants.NOOP_ROOT;
            }
        }
    }
}