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
    using PathConstants = GraphQL.AspNet.Constants.Routing;

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
        internal static string ToPathRootString(this ItemPathRoots value)
        {
            switch (value)
            {
                case ItemPathRoots.Query:
                    return PathConstants.QUERY_ROOT;

                case ItemPathRoots.Mutation:
                    return PathConstants.MUTATION_ROOT;

                case ItemPathRoots.Subscription:
                    return PathConstants.SUBSCRIPTION_ROOT;

                case ItemPathRoots.Types:
                    return PathConstants.TYPE_ROOT;

                case ItemPathRoots.Directives:
                    return PathConstants.DIRECTIVE_ROOT;

                default:
                    return PathConstants.NOOP_ROOT;
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