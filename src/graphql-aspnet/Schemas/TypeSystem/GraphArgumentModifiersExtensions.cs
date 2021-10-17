﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    /// <summary>
    /// Extension helper methods for <see cref="GraphArgumentModifiers"/>.
    /// </summary>
    public static class GraphArgumentModifiersExtensions
    {
        /// <summary>
        /// Determines whether the modifers indicate the argument is to contain the source data value supplied to the resolver for the field.
        /// </summary>
        /// <param name="modifiers">The modifiers set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the parent field reslt modifer.</returns>
        public static bool IsSourceParameter(this GraphArgumentModifiers modifiers)
        {
            return modifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult);
        }

        /// <summary>
        /// Determines whether the modifers indicate the argument is internal and not part of the graph.
        /// </summary>
        /// <param name="modifiers">The modifiers set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the internal modifer.</returns>
        public static bool IsInternalParameter(this GraphArgumentModifiers modifiers)
        {
            return modifiers.HasFlag(GraphArgumentModifiers.Internal);
        }
    }
}