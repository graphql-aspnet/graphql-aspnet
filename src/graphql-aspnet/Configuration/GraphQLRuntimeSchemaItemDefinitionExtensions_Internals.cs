// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.Templates;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLRuntimeSchemaItemDefinitionExtensions
    {
        private static IGraphQLRuntimeFieldDefinition MapGraphQLFieldInternal(
            SchemaOptions schemaOptions,
            GraphOperationType operationType,
            string pathTemplate)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            pathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(pathTemplate, nameof(pathTemplate));

            var path = new SchemaItemPath((SchemaItemCollections)operationType, pathTemplate);

            var fieldTemplate = new RuntimeVirtualFieldTemplate(
                schemaOptions,
                path.Path);

            return fieldTemplate;
        }

        private static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtensionInternal(
            SchemaOptions schemaOptions,
            Type typeToExtend,
            string fieldName,
            FieldResolutionMode resolutionMode)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            fieldName = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));

            IGraphQLRuntimeTypeExtensionDefinition field = new RuntimeTypeExtensionFieldDefinition(
                schemaOptions,
                typeToExtend,
                fieldName,
                resolutionMode);

            schemaOptions.AddSchemaItemTemplate(field);
            return field;
        }

        private static IGraphQLRuntimeDirectiveDefinition MapDirectiveInternal(this SchemaOptions schemaOptions, string directiveName)
        {
            var directive = new RuntimeDirectiveDefinition(schemaOptions, directiveName);

            schemaOptions.AddSchemaItemTemplate(directive);
            return directive;
        }
    }
}