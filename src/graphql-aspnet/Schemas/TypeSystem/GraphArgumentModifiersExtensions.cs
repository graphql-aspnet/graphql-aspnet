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
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// Extension helper methods for <see cref="ParameterModifiers"/>.
    /// </summary>
    public static class GraphArgumentModifiersExtensions
    {
        /// <summary>
        /// Determines whether the modifers indicate the argument is to contain the context of
        /// the directive or field being resolved by the target resolver.
        /// </summary>
        /// <param name="modifiers">The modifiers to check.</param>
        /// <returns><c>true</c> if the parameters represent the resolver context; otherwise, <c>false</c>.</returns>
        public static bool IsResolverContext(this ParameterModifiers modifiers)
        {
            return modifiers == ParameterModifiers.ResolutionContext;
        }

        /// <summary>
        /// Determines whether the modifers indicate the argument is to contain the source data value supplied to the resolver for the field.
        /// </summary>
        /// <param name="modifier">The modifier set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the parent field reslt modifer.</returns>
        public static bool IsSourceParameter(this ParameterModifiers modifier)
        {
            return modifier == ParameterModifiers.ParentFieldResult;
        }

        /// <summary>
        /// Determines whether the modifers indicate the argument is a reference
        /// to the cancellation token governing the overall request.
        /// </summary>
        /// <param name="modifier">The modifier set to check.</param>
        /// <returns><c>true</c> if the modifers set declares the internal modifer.</returns>
        public static bool IsCancellationToken(this ParameterModifiers modifier)
        {
            return modifier == ParameterModifiers.CancellationToken;
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is included in a
        /// an externally exposed schema.
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the modifier indicate the argument is part of the schema; otherwise, <c>false</c>.</returns>
        public static bool CouldBePartOfTheSchema(this ParameterModifiers modifier)
        {
            return modifier == ParameterModifiers.None ||
                   modifier.IsExplicitlyPartOfTheSchema();
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is explicitly declared that it MUST be included in a
        /// an externally exposed schema.
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the modifier indicate the argument is explicitly declared to be a part of the schema; otherwise, <c>false</c>.</returns>
        public static bool IsExplicitlyPartOfTheSchema(this ParameterModifiers modifier)
        {
            return modifier == ParameterModifiers.ExplicitSchemaItem;
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is to be resolved from a DI
        /// container as opposed to being passed ona  query
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the modifier indicate the argument is to be resolved from a DI continer; otherwise, <c>false</c>.</returns>
        public static bool IsInjected(this ParameterModifiers modifier)
        {
            return modifier == ParameterModifiers.ExplicitInjected ||
                modifier == ParameterModifiers.ImplicitInjected;
        }

        /// <summary>
        /// Determines whether the modifier indicate that the argument is to be populated with the http context for the request.
        /// </summary>
        /// <param name="modifier">The modifier to check.</param>
        /// <returns><c>true</c> if the parameters represent the global http context for the request; otherwise, <c>false</c>.</returns>
        public static bool IsHttpContext(this ParameterModifiers modifier)
        {
            return modifier == ParameterModifiers.HttpContext;
        }
    }
}