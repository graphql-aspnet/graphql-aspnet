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
    /// <summary>
    /// An internal overload of <see cref="ItemPath"/> that allows reserved names
    /// as part of the path segment.
    /// </summary>
    internal sealed class IntrospectedItemPath : ItemPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedItemPath"/> class.
        /// </summary>
        /// <param name="collection">The collection the path belongs to.</param>
        /// <param name="pathSegments">The individual segments of the path.</param>
        public IntrospectedItemPath(Execution.ItemPathRoots collection, params string[] pathSegments)
            : base(collection, pathSegments)
        {
        }

        /// <summary>
        /// Validates that the given path fragment is valid and usable.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        /// <returns>System.Boolean.</returns>
        protected override bool ValidateFragment(string fragment)
        {
            if (Constants.ReservedNames.IntrospectablePathNames.Contains(fragment))
                return true;

            return Constants.RegExPatterns.NameRegex.IsMatch(fragment);
        }
    }
}