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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DirectiveExecution.DirectiveValidation;
    using GraphQL.AspNet.Interfaces.Configuration;
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
        private readonly ISchemaConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphTypeMaker" /> class.
        /// </summary>
        /// <param name="config">The schema configuration to use when building the graph type.</param>
        public EnumGraphTypeMaker(ISchemaConfiguration config)
        {
            _config = Validation.ThrowIfNullOrReturn(config, nameof(config));
        }

        /// <inheritdoc />
        public virtual GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate)
        {
            if (!(typeTemplate is IEnumGraphTypeTemplate template))
                return null;

            var requirements = template.DeclarationRequirements ?? _config.DeclarationOptions.FieldDeclarationRequirements;

            // enum level directives
            var enumDirectives = template.CreateAppliedDirectives();

            var graphType = new EnumGraphType(
                _config.DeclarationOptions.GraphNamingFormatter.FormatGraphTypeName(template.Name),
                template.InternalName,
                template.ObjectType,
                template.Route,
                enumDirectives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            var result = new GraphTypeCreationResult()
            {
                GraphType = graphType,
                ConcreteType = template.ObjectType,
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
                    _config.DeclarationOptions.GraphNamingFormatter.FormatEnumValueName(value.Name),
                    value.InternalName,
                    value.Description,
                    value.Route,
                    value.Value,
                    value.DeclaredLabel,
                    valueDirectives);

                graphType.AddOption(valueOption);

                result.AddDependentRange(value.RetrieveRequiredTypes());
            }

            return result;
        }
    }
}