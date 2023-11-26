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
    using PathConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// A representation of a hierarchical path to a single field in within a graph schema.
    /// </summary>
    public partial class ItemPath
    {
        /// <summary>
        /// Joins a parent and child path segments under the top level field type provided.
        /// </summary>
        /// <param name="pathSegments">The path segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(params string[] pathSegments)
        {
            var fragment = string.Join(PathConstants.PATH_SEPERATOR, pathSegments);
            return ItemPath.NormalizeFragment(fragment);
        }

        /// <summary>
        /// Joins a parent and child path segments under the top level field type provided.
        /// </summary>
        /// <param name="fieldType">Type of the field to prepend a root key to the path.</param>
        /// <param name="pathSegments">The path segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(ItemPathRoots fieldType, params string[] pathSegments)
        {
            return ItemPath.Join(fieldType.ToPathRootString().AsEnumerable().Concat(pathSegments).ToArray());
        }

        /// <summary>
        /// Normalizes a given path fragement removing duplicate seperators, ensuring starting and tail end seperators
        /// are correct etc.
        /// </summary>
        /// <param name="pathfragment">The fragment to normalize.</param>
        /// <returns>System.String.</returns>
        public static string NormalizeFragment(string pathfragment)
        {
            // ensure a working path
            pathfragment = pathfragment?.Trim() ?? string.Empty;
            pathfragment = pathfragment.Replace(PathConstants.ALT_PATH_SEPERATOR, PathConstants.PATH_SEPERATOR);

            // doubled up seperators may happen if a 3rd party is joining path fragments (especially if the seperator is a '/')
            // trim them down
            while (pathfragment.Contains(PathConstants.DOUBLE_PATH_SEPERATOR))
            {
                pathfragment = pathfragment.Replace(PathConstants.DOUBLE_PATH_SEPERATOR, PathConstants.PATH_SEPERATOR);
            }

            // if the path ends with or starts with a seperator (indicating a potential group segment)
            // thats fine but is not needed to identify the segment in and of itself, trim it off
            while (pathfragment.EndsWith(PathConstants.PATH_SEPERATOR))
            {
                pathfragment = pathfragment.Substring(0, pathfragment.Length - PathConstants.PATH_SEPERATOR.Length);
            }

            while (pathfragment.StartsWith(PathConstants.PATH_SEPERATOR))
            {
                pathfragment = pathfragment.Substring(pathfragment.IndexOf(PathConstants.PATH_SEPERATOR, StringComparison.Ordinal) + PathConstants.PATH_SEPERATOR.Length);
            }

            return pathfragment;
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ItemPath left, ItemPath right)
        {
            return left?.Equals(right) ?? right?.Equals(left) ?? true;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(ItemPath left, ItemPath right)
        {
            return !(left == right);
        }
    }
}