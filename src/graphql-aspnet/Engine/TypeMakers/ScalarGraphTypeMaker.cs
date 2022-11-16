// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults.TypeMakers
{
    using System;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IScalarGraphType"/> from its related template.
    /// </summary>
    public class ScalarGraphTypeMaker : IGraphTypeMaker
    {
        /// <summary>
        /// Inspects the given type and, in accordance with the rules of this maker, will
        /// generate a complete set of necessary graph types required to support it.
        /// </summary>
        /// <param name="concreteType">The concrete type to incorporate into the schema.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        public GraphTypeCreationResult CreateGraphType(Type concreteType)
        {
            var scalarType = GraphQLProviders.ScalarProvider.CreateScalar(concreteType);
            if (scalarType != null)
            {
                var result = new GraphTypeCreationResult()
                {
                    GraphType = scalarType,
                    ConcreteType = concreteType,
                };

                // add any known diretives as dependents
                foreach (var directiveToApply in scalarType.AppliedDirectives)
                {
                    if (directiveToApply.DirectiveType != null)
                        result.AddDependent(directiveToApply.DirectiveType, TypeKind.DIRECTIVE);
                }

                return result;
            }

            return null;
        }
    }
}