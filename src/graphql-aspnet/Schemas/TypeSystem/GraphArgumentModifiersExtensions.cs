// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System.Runtime.CompilerServices;

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

        /// <summary>
        /// Determines whether the modifers indicate the argument is a reference
        /// to the cancellation token governing the overall request.
        /// </summary>
        /// <param name="modifiers">The modifiers set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the internal modifer.</returns>
        public static bool IsCancellationToken(this GraphArgumentModifiers modifiers)
        {
            return modifiers.HasFlag(GraphArgumentModifiers.CancellationToken);
        }

        /// <summary>
        /// Determines whether the modifiers indicate that the argument is, for one reason or another,
        /// not part of the externally exposed schema. This method cannot determine
        /// what special type of argument is represented, only that it is special.
        /// </summary>
        /// <param name="modifiers">The modifiers to check.</param>
        /// <returns><c>true</c> if the modifiers indicate the argument is not part of the schema; otherwise, <c>false</c>.</returns>
        public static bool IsNotPartOfTheSchema(this GraphArgumentModifiers modifiers)
        {
            return modifiers != GraphArgumentModifiers.None;
        }
    }
}