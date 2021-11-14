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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// An implemention of the route path system with special considering for input arguments
    /// to differentiate them from normal field routes.
    /// </summary>
    public class GraphArgumentFieldPath : GraphFieldPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphArgumentFieldPath" /> class.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="argumentName">Name of the argument.</param>
        public GraphArgumentFieldPath(GraphFieldPath parentPath, string argumentName)
            : base(GraphFieldPath.Join(parentPath.Path, argumentName))
        {
        }

        /// <summary>
        /// Generates the fully qualified path string of this instance.
        /// </summary>
        /// <param name="pathFragments">The path fragments to join.</param>
        /// <returns>System.String.</returns>
        protected override string GeneratePathString(IReadOnlyList<string> pathFragments)
        {
            // arguments should always be at least 2 to be valid an owner and the arg itself
            // and will usually be 4:  [query]/someObject/someMethod[argName]
            if (pathFragments.Count < 2)
                return base.GeneratePathString(pathFragments);

            var lastItem = pathFragments[pathFragments.Count - 1];
            return string.Join(RouteConstants.PATH_SEPERATOR, pathFragments.SkipLastN(1))
                + RouteConstants.DELIMITER_ROOT_START + lastItem + RouteConstants.DELIMITER_ROOT_END;
        }
    }
}