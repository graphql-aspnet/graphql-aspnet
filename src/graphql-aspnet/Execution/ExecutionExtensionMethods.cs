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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// Helper methods useful during document execution.
    /// </summary>
    public static class ExecutionExtensionMethods
    {
        /// <summary>
        /// Converts the supplied collection value into its equivilant routing constant.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>System.String.</returns>
        internal static string ToRouteRoot(this SchemaItemPathCollections value)
        {
            switch (value)
            {
                case SchemaItemPathCollections.Query:
                    return RouteConstants.QUERY_ROOT;

                case SchemaItemPathCollections.Mutation:
                    return RouteConstants.MUTATION_ROOT;

                case SchemaItemPathCollections.Subscription:
                    return RouteConstants.SUBSCRIPTION_ROOT;

                case SchemaItemPathCollections.Types:
                    return RouteConstants.TYPE_ROOT;

                case SchemaItemPathCollections.Enums:
                    return RouteConstants.ENUM_ROOT;

                case SchemaItemPathCollections.Directives:
                    return RouteConstants.DIRECTIVE_ROOT;

                default:
                    return RouteConstants.NOOP_ROOT;
            }
        }

        /// <summary>
        /// Extends the provided resolver by executing the supplied function
        /// after the initial resolver completes. This functon is executed regardless
        /// of the state of the context; successfully completed or failed.
        /// </summary>
        /// <param name="resolver">The base resolver to extend.</param>
        /// <param name="extensionFunction">A function to extend the resolver with.</param>
        /// <returns>The extended resolver.</returns>
        public static IGraphFieldResolver Extend(
            this IGraphFieldResolver resolver,
            Func<FieldResolutionContext, CancellationToken, Task> extensionFunction)
        {
            return new ExtendedGraphFieldResolver(resolver, extensionFunction);
        }
    }
}