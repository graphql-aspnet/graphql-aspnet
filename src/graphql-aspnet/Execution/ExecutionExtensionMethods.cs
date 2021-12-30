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
    using GraphQL.AspNet.Execution.ValueResolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// Helper methods useful during document execution.
    /// </summary>
    public static class ExecutionExtensionMethods
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

        /// <summary>
        /// Extends the resolver, executing the supplied function
        /// after the resolver completes. This method is executed regardless
        /// of the state of the context (successfully completed or failed).
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>IGraphFieldResolver.</returns>
        public static IGraphFieldResolver Extend(
            this IGraphFieldResolver resolver,
            Func<FieldResolutionContext, CancellationToken, Task> extension)
        {
            return new ExtendedResolver(resolver, extension);
        }
    }
}