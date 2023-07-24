﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphArgumentTemplate"/> into a usable <see cref="IGraphArgument"/> on a graph field.
    /// </summary>
    public class GraphArgumentMaker : IGraphArgumentMaker
    {
        private readonly ISchema _schema;
        private readonly ISchemaConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphArgumentMaker" /> class.
        /// </summary>
        /// <param name="schema">The schema being built.</param>
        public GraphArgumentMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _config = _schema.Configuration;
        }

        /// <inheritdoc />
        public GraphArgumentCreationResult CreateArgument(ISchemaItem owner, IGraphArgumentTemplate template)
        {
            Validation.ThrowIfNull(owner, nameof(owner));
            var formatter = _config.DeclarationOptions.GraphNamingFormatter;

            var directives = template.CreateAppliedDirectives();

            // all arguments are either leafs or input objects
            var existingGraphType = _schema.KnownTypes.FindGraphType(template.ObjectType, TypeKind.INPUT_OBJECT);

            string schemaTypeName;
            if (existingGraphType != null && existingGraphType.Kind.IsValidInputKind())
            {
                // when the type already exists on the target schema
                // and is usable as an input type then just use the name
                schemaTypeName = existingGraphType.Name;
            }
            else
            {
                // guess on what the name of the schema item will be
                // this is guaranteed correct for all but scalars and scalars should be
                // added first
                schemaTypeName = GraphTypeNames.ParseName(template.ObjectType, TypeKind.INPUT_OBJECT);
            }

            // enforce non-renaming standards in the maker since the
            // directly controls the formatter
            if (GlobalTypes.CanBeRenamed(schemaTypeName))
                schemaTypeName = formatter.FormatGraphTypeName(schemaTypeName);

            var typeExpression = template.TypeExpression.CloneTo(schemaTypeName);

            var argument = new GraphFieldArgument(
                owner,
                formatter.FormatFieldName(template.Name),
                template.InternalName,
                template.ParameterName,
                typeExpression,
                template.Route,
                template.ObjectType,
                template.HasDefaultValue,
                template.DefaultValue,
                template.Description,
                directives);

            var result = new GraphArgumentCreationResult();
            result.Argument = argument;

            result.AddDependentRange(template.RetrieveRequiredTypes());

            return result;
        }

        /// <summary>
        /// Determines whether the provided argument template should be included as part of the schema.
        /// </summary>
        /// <param name="argTemplate">The argument template to evaluate.</param>
        /// <param name="schema">The schema to evaluate against.</param>
        /// <returns><c>true</c> if the template should be rendered into the schema; otherwise, <c>false</c>.</returns>
        public static bool IsArgumentPartOfSchema(IGraphArgumentTemplate argTemplate, ISchema schema)
        {
            Validation.ThrowIfNull(argTemplate, nameof(argTemplate));
            Validation.ThrowIfNull(schema, nameof(schema));

            if (argTemplate.ArgumentModifier.IsExplicitlyPartOfTheSchema())
                return true;

            if (!argTemplate.ArgumentModifier.CouldBePartOfTheSchema())
                return false;

            // teh argument contains no explicit inclusion or exclusion modifiers
            // what do we do with it?
            switch (schema.Configuration.DeclarationOptions.ArgumentBindingRule)
            {
                case Configuration.SchemaArgumentBindingRules.ParametersRequireFromGraphQLDeclaration:

                    // arg didn't explicitly have [FromGraphQL] so it should NOT be part of the schema
                    return false;

                case Configuration.SchemaArgumentBindingRules.ParametersRequireFromServicesDeclaration:

                    // arg didn't explicitly have [FromServices] so it should be part of the schema
                    return true;

                case Configuration.SchemaArgumentBindingRules.ParametersPreferQueryResolution:
                default:

                    // only exclude types that could never be correct as an input argument
                    // ---
                    // interfaces can never be valid input object types
                    if (argTemplate.ObjectType.IsInterface)
                        return false;

                    return true;
            }
        }
    }
}