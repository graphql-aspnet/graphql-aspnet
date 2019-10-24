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
    using System.Collections.Generic;

    /// <summary>
    /// Helper methods for <see cref="TypeExpressions"/>.
    /// </summary>
    public static class TypeExpressionsExtensions
    {
        /// <summary>
        /// Converts the provided <see cref="TypeExpressions"/> into an equivilant <see cref="MetaGraphTypes"/>
        /// array.
        /// </summary>
        /// <param name="modifer">The modifer to expand.</param>
        /// <returns>GraphTypeWrapper[].</returns>
        public static MetaGraphTypes[] ToTypeWrapperSet(this TypeExpressions modifer)
        {
            var list = new List<MetaGraphTypes>();

            if (modifer.HasFlag(TypeExpressions.IsNotNullList))
                list.Add(MetaGraphTypes.IsNotNull);

            if (modifer.HasFlag(TypeExpressions.IsList))
                list.Add(MetaGraphTypes.IsList);

            if (modifer.HasFlag(TypeExpressions.IsNotNull) && (list.Count == 0 || list[list.Count - 1] != MetaGraphTypes.IsNotNull))
                list.Add(MetaGraphTypes.IsNotNull);

            return list.ToArray();
        }
    }
}