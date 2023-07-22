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
    /// <summary>
    /// Extension helper methods for <see cref="GraphArgumentModifiers"/>.
    /// </summary>
    public static class GraphArgumentModifiersExtensions
    {
        /// <summary>
        /// Determines whether the modifers indicate the argument is to contain the context of
        /// the directive or field being resolved by the target resolver.
        /// </summary>
        /// <param name="modifiers">The modifiers to check.</param>
        /// <returns><c>true</c> if the parameters represent the resolver context; otherwise, <c>false</c>.</returns>
        public static bool IsResolverContext(this GraphArgumentModifiers modifiers)
        {
            return modifiers == GraphArgumentModifiers.ResolutionContext;
        }

        /// <summary>
        /// Determines whether the modifers indicate the argument is to contain the source data value supplied to the resolver for the field.
        /// </summary>
        /// <param name="modifier">The modifier set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the parent field reslt modifer.</returns>
        public static bool IsSourceParameter(this GraphArgumentModifiers modifier)
        {
            return modifier == GraphArgumentModifiers.ParentFieldResult;
        }

        /// <summary>
        /// Determines whether the modifers indicate the argument is a reference
        /// to the cancellation token governing the overall request.
        /// </summary>
        /// <param name="modifier">The modifier set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the internal modifer.</returns>
        public static bool IsCancellationToken(this GraphArgumentModifiers modifier)
        {
            return modifier == GraphArgumentModifiers.CancellationToken;
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is included in a
        /// an externally exposed schema.
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the modifier indicate the argument is part of the schema; otherwise, <c>false</c>.</returns>
        public static bool CouldBePartOfTheSchema(this GraphArgumentModifiers modifier)
        {
            return modifier == GraphArgumentModifiers.None ||
                   modifier.IsExplicitlyPartOfTheSchema();
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is explicitly declared that it MUST be included in a
        /// an externally exposed schema.
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the modifier indicate the argument is explicitly declared to be a part of the schema; otherwise, <c>false</c>.</returns>
        public static bool IsExplicitlyPartOfTheSchema(this GraphArgumentModifiers modifier)
        {
            return modifier == GraphArgumentModifiers.ExplicitSchemaItem;
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is to be resolved from a DI
        /// container as opposed to being passed ona  query
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the modifier indicate the argument is to be resolved from a DI continer; otherwise, <c>false</c>.</returns>
        public static bool IsInjected(this GraphArgumentModifiers modifier)
        {
            return modifier == GraphArgumentModifiers.ExplicitInjected;
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is to be populated with the http context for the request.
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the parameters represent the global http context for the request; otherwise, <c>false</c>.</returns>
        public static bool IsHttpContext(this GraphArgumentModifiers modifier)
        {
            return modifier == GraphArgumentModifiers.HttpContext;
        }
    }
}