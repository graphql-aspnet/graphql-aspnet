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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object responsible for extracting appropriate enumeration data from a template and generating a <see cref="IEnumGraphType"/> that
    /// can be added to a schema.
    /// </summary>
    public class EnumGraphTypeMaker : IGraphTypeMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphTypeMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema defining the graph type creation rules this generator should follow.</param>
        public EnumGraphTypeMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Inspects the given type and, in accordance with the rules of this maker, will
        /// generate a complete set of necessary graph types required to support it.
        /// </summary>
        /// <param name="concreteType">The concrete type to incorporate into the schema.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        public GraphTypeCreationResult CreateGraphType(Type concreteType)
        {
            var template = GraphQLProviders.TemplateProvider.ParseType(concreteType, TypeKind.ENUM) as IEnumGraphTypeTemplate;
            if (template == null)
                return null;

            var requirements = template.DeclarationRequirements ?? _schema.Configuration.DeclarationOptions.FieldDeclarationRequirements;

            var graphType = new EnumGraphType(
                _schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatGraphTypeName(template.Name),
                concreteType)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            // clone each option using the formatter supplied by the schema configuration
            var enumValuesToInclude = template.Values.Where(value => value.IsExplicitlyDeclared || requirements.AllowImplicitEnumValues());
            foreach (var value in enumValuesToInclude)
            {
                var modifiedValue = new GraphEnumOption(
                    concreteType,
                    _schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatEnumValueName(value.Name),
                    value.Description,
                    value.IsExplicitlyDeclared,
                    value.IsDeprecated,
                    value.DeprecationReason);

                graphType.AddOption(modifiedValue);
            }

            return new GraphTypeCreationResult()
            {
                GraphType = graphType,
                ConcreteType = concreteType,
            };
        }
    }
}