// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// A representation of a hierarchical path to a single field in within a graph schema.
    /// </summary>
    public partial class GraphFieldPath
    {
        /// <summary>
        /// Joins a parent and child route segments under the top level field type provided.
        /// </summary>
        /// <param name="routeSegments">The route segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(params string[] routeSegments)
        {
            var fragment = string.Join(RouteConstants.PATH_SEPERATOR, routeSegments);
            return GraphFieldPath.NormalizeFragment(fragment);
        }

        /// <summary>
        /// Joins a parent and child route segments under the top level field type provided.
        /// </summary>
        /// <param name="fieldType">Type of the field to prepend a root key to the path.</param>
        /// <param name="routeSegments">The route segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(GraphCollection fieldType, params string[] routeSegments)
        {
            return GraphFieldPath.Join(fieldType.ToRouteRoot().AsEnumerable().Concat(routeSegments).ToArray());
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GraphFieldPath left, GraphFieldPath right)
        {
            return left?.Equals(right) ?? right?.Equals(left) ?? true;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(GraphFieldPath left, GraphFieldPath right)
        {
            return !(left == right);
        }
    }
}