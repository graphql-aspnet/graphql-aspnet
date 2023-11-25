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
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// A representation of a hierarchical path to a single field in within a graph schema.
    /// </summary>
    public partial class SchemaItemPath
    {
        /// <summary>
        /// Joins a parent and child route segments under the top level field type provided.
        /// </summary>
        /// <param name="routeSegments">The route segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(params string[] routeSegments)
        {
            var fragment = string.Join(RouteConstants.PATH_SEPERATOR, routeSegments);
            return SchemaItemPath.NormalizeFragment(fragment);
        }

        /// <summary>
        /// Joins a parent and child route segments under the top level field type provided.
        /// </summary>
        /// <param name="fieldType">Type of the field to prepend a root key to the path.</param>
        /// <param name="routeSegments">The route segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(SchemaItemPathCollections fieldType, params string[] routeSegments)
        {
            return SchemaItemPath.Join(fieldType.ToRouteRoot().AsEnumerable().Concat(routeSegments).ToArray());
        }

        /// <summary>
        /// Normalizes a given route path fragement removing duplicate seperators, ensuring starting and tail end seperators
        /// are correct etc.
        /// </summary>
        /// <param name="routefragment">The fragment to normalize.</param>
        /// <returns>System.String.</returns>
        public static string NormalizeFragment(string routefragment)
        {
            // ensure a working path
            routefragment = routefragment?.Trim() ?? string.Empty;
            routefragment = routefragment.Replace(RouteConstants.ALT_PATH_SEPERATOR, RouteConstants.PATH_SEPERATOR);

            // doubled up seperators may happen if a 3rd party is joining route fragments (especially if the seperator is a '/')
            // trim them down
            while (routefragment.Contains(RouteConstants.DOUBLE_PATH_SEPERATOR))
            {
                routefragment = routefragment.Replace(RouteConstants.DOUBLE_PATH_SEPERATOR, RouteConstants.PATH_SEPERATOR);
            }

            // if the path ends with or starts with a seperator (indicating a potential group segment)
            // thats fine but is not needed to identify the segment in and of itself, trim it off
            while (routefragment.EndsWith(RouteConstants.PATH_SEPERATOR))
            {
                routefragment = routefragment.Substring(0, routefragment.Length - RouteConstants.PATH_SEPERATOR.Length);
            }

            while (routefragment.StartsWith(RouteConstants.PATH_SEPERATOR))
            {
                routefragment = routefragment.Substring(routefragment.IndexOf(RouteConstants.PATH_SEPERATOR, StringComparison.Ordinal) + RouteConstants.PATH_SEPERATOR.Length);
            }

            return routefragment;
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SchemaItemPath left, SchemaItemPath right)
        {
            return left?.Equals(right) ?? right?.Equals(left) ?? true;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(SchemaItemPath left, SchemaItemPath right)
        {
            return !(left == right);
        }
    }
}