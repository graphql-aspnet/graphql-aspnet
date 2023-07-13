// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IScalarGraphType"/> from its related template.
    /// </summary>
    public class ScalarGraphTypeMaker : IGraphTypeMaker
    {
        private readonly ISchemaConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarGraphTypeMaker"/> class.
        /// </summary>
        /// <param name="config">The configuration object used for configuring scalar types.</param>
        public ScalarGraphTypeMaker(ISchemaConfiguration config)
        {
            _config = Validation.ThrowIfNullOrReturn(config, nameof(config));
        }

        /// <inheritdoc />
        public GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate)
        {
            if (!(typeTemplate is IScalarGraphTypeTemplate scalarTemplate))
                return null;

            var scalarType = GlobalTypes.CreateScalarInstanceOrThrow(scalarTemplate.ScalarType);
            scalarType = scalarType.Clone(_config.DeclarationOptions.GraphNamingFormatter.FormatGraphTypeName(scalarType.Name));

            var result = new GraphTypeCreationResult()
            {
                GraphType = scalarType,
                ConcreteType = scalarType.ObjectType,
            };

            // add any known directives as dependents
            // to be added to the schema
            foreach (var directiveToApply in scalarType.AppliedDirectives)
            {
                if (directiveToApply.DirectiveType != null)
                    result.AddDependent(directiveToApply.DirectiveType, TypeKind.DIRECTIVE);
            }

            return result;
        }
    }
}