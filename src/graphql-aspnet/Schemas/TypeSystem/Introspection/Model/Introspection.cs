// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Helper methods used during introspection model building.
    /// </summary>
    public static class Introspection
    {
        /// <summary>
        /// Wraps the base type with any necessary modifier types to generate the full type declaration.
        /// </summary>
        /// <param name="baseType">The base type to wrap.</param>
        /// <param name="wrappers">The wrappers.</param>
        /// <returns>IIntrospectedType.</returns>
        public static IntrospectedType WrapBaseTypeWithModifiers(IntrospectedType baseType, params MetaGraphTypes[] wrappers)
        {
            return Introspection.WrapBaseTypeWithModifiers(baseType, wrappers.ToList());
        }

        /// <summary>
        /// Wraps the base type with any necessary modifier types to generate the full type declaration.
        /// </summary>
        /// <param name="baseType">The base type to wrap.</param>
        /// <param name="wrappers">The wrappers.</param>
        /// <returns>IIntrospectedType.</returns>
        public static IntrospectedType WrapBaseTypeWithModifiers(IntrospectedType baseType, IReadOnlyList<MetaGraphTypes> wrappers)
        {
            for (var i = wrappers.Count - 1; i >= 0; i--)
            {
                switch (wrappers[i])
                {
                    case MetaGraphTypes.IsNotNull:
                        baseType = new IntrospectedType(baseType, TypeKind.NON_NULL);
                        break;

                    case MetaGraphTypes.IsList:
                        baseType = new IntrospectedType(baseType, TypeKind.LIST);
                        break;
                }
            }

            return baseType;
        }

        /// <summary>
        /// Wraps the base type with any necessary modifier types to generate the full type declaration.
        /// </summary>
        /// <param name="baseType">The base type to wrap.</param>
        /// <param name="typeExpression">The type expression.</param>
        /// <returns>IIntrospectedType.</returns>
        public static IntrospectedType WrapBaseTypeWithModifiers(IntrospectedType baseType, GraphTypeExpression typeExpression)
        {
            return Introspection.WrapBaseTypeWithModifiers(baseType, typeExpression.Wrappers);
        }
    }
}