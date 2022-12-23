// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine.TypeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
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
        /// <param name="schema">The schema this generator should follow.</param>
        public EnumGraphTypeMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public GraphTypeCreationResult CreateGraphType(Type concreteType)
        {
            var template = GraphQLProviders.TemplateProvider.ParseType(concreteType, TypeKind.ENUM) as IEnumGraphTypeTemplate;
            if (template == null)
                return null;

            var requirements = template.DeclarationRequirements ?? _schema.Configuration.DeclarationOptions.FieldDeclarationRequirements;

            // enum level directives
            var enumDirectives = template.CreateAppliedDirectives();

            var graphType = new EnumGraphType(
                _schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatGraphTypeName(template.Name),
                concreteType,
                template.Route,
                enumDirectives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            var result = new GraphTypeCreationResult()
            {
                GraphType = graphType,
                ConcreteType = concreteType,
            };

            // account for any potential type system directives
            result.AddDependentRange(template.RetrieveRequiredTypes());

            // create an enum option from each template
            var enumValuesToInclude = template.Values.Where(value => requirements.AllowImplicitEnumValues() || value.IsExplicitDeclaration);
            foreach (var value in enumValuesToInclude)
            {
                // enum option directives
                var valueDirectives = value.CreateAppliedDirectives();

                var valueOption = new EnumValue(
                    graphType,
                    _schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatEnumValueName(value.Name),
                    value.Description,
                    value.Route,
                    value.Value,
                    value.InternalName,
                    valueDirectives);

                graphType.AddOption(valueOption);

                result.AddDependentRange(value.RetrieveRequiredTypes());
            }

            return result;
        }
    }
}