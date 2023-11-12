﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// Helper methods for working with <see cref="ResolverIsolationOptions"/>.
    /// </summary>
    public static class ResolverIsolationOptionsExtensions
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
            switch (fieldSource)
            {
                case GraphFieldSource.None:
                case GraphFieldSource.Virtual:
                    return false;

                case GraphFieldSource.Action:
                    return (options & ResolverIsolationOptions.ControllerActions) > 0;

                case GraphFieldSource.Method:
                    return (options & ResolverIsolationOptions.Methods) > 0;

                case GraphFieldSource.Property:
                    return (options & ResolverIsolationOptions.Properties) > 0;

                default:
                    throw new InvalidOperationException($"Unsupported field source {fieldSource}");
            }
        }
    }
}