// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection
{
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An internal overload of <see cref="GraphFieldPath"/> that allows reserved names
    /// as part of the path segment.
    /// </summary>
    internal sealed class IntrospectedRoutePath : GraphFieldPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedRoutePath"/> class.
        /// </summary>
        /// <param name="collection">The collection the route belongs to.</param>
        /// <param name="pathSegments">The individual path segments of the route.</param>
        public IntrospectedRoutePath(Execution.GraphCollection collection, params string[] pathSegments)
            : base(collection, pathSegments)
        {
        }

        /// <summary>
        /// Validates that the given route fragment is valid and usable.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        /// <returns>System.Boolean.</returns>
        protected override bool ValidateFragment(string fragment)
        {
            if (Constants.ReservedNames.IntrospectableRouteNames.Contains(fragment))
                return true;

            return Constants.RegExPatterns.NameRegex.IsMatch(fragment);
        }
    }
}