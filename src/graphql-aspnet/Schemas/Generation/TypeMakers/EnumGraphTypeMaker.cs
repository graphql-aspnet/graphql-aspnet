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
    using GraphQL.AspNet.Execution.Exceptions;
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

            template.Parse();
            template.ValidateOrThrow(false);

            var requirements = template.DeclarationRequirements ?? _config.DeclarationOptions.FieldDeclarationRequirements;

            // enum level directives
            var enumDirectives = template.CreateAppliedDirectives();

            var graphType = new EnumGraphType(
                template.Name,
                template.InternalName,
                template.ObjectType,
                template.ItemPath,
                enumDirectives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            graphType = _config
                .DeclarationOptions?
                .SchemaFormatStrategy?
                .ApplyFormatting(_config, graphType) ?? graphType;

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
                value.Parse();
                value.ValidateOrThrow(false);

                // enum option directives
                var valueDirectives = value.CreateAppliedDirectives();

                var valueOption = new EnumValue(
                    graphType,
                    value.Name,
                    value.InternalName,
                    value.Description,
                    value.ItemPath,
                    value.Value,
                    value.DeclaredLabel,
                    valueDirectives);

                valueOption = _config
                    .DeclarationOptions?
                    .SchemaFormatStrategy?
                    .ApplyFormatting(_config, valueOption) ?? valueOption;

                if (Constants.QueryLanguage.IsReservedKeyword(valueOption.Name))
                {
                    throw new GraphTypeDeclarationException($"The enum value '{value.Name}' is invalid for " +
                        $"graph type '{graphType.Name}' on the target schema. {value.Name} is a reserved keyword.");
                }

                graphType.AddOption(valueOption);

                result.AddDependentRange(value.RetrieveRequiredTypes());
            }

            return result;
        }
    }
}