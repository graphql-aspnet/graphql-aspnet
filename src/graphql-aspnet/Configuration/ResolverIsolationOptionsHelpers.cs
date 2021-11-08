// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// Helper methods for working with <see cref="ResolverIsolationOptions"/>.
    /// </summary>
    public static class ResolverIsolationOptionsHelpers
    {
        /// <summary>
        /// Indicates if the given <paramref name="fieldSource"/> is one that should be
        /// isolated accorind to the supplied <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options collection to inspect.</param>
        /// <param name="fieldSource">The field source to test.</param>
        /// <returns><c>true</c> if the field source should be isolated, <c>false</c> otherwise.</returns>
        public static bool ShouldIsolateFieldSource(this ResolverIsolationOptions options, GraphFieldSource fieldSource)
        {
            if (fieldSource == GraphFieldSource.Virtual)
                return false;

            return options.HasFlag((ResolverIsolationOptions)fieldSource);
        }
    }
}