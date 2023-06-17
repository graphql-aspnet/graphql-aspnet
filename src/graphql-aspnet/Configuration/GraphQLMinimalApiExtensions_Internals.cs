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
    public static partial class GraphQLMinimalApiExtensions
    {
        private static IGraphQLFieldTemplate MapGraphQLFieldInternal(
            SchemaOptions schemaOptions,
            GraphOperationType operationType,
            string pathTemplate)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            pathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(pathTemplate, nameof(pathTemplate));

            var path = new SchemaItemPath((SchemaItemCollections)operationType, pathTemplate);

            var fieldTemplate = new GraphQLVirtualFieldTemplate(
                schemaOptions,
                path.Path);

            return fieldTemplate;
        }

        private static IGraphQLTypeExtensionTemplate MapTypeExtensionInternal(
            SchemaOptions schemaOptions,
            Type typeToExtend,
            string fieldName,
            FieldResolutionMode resolutionMode)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            fieldName = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));

            IGraphQLTypeExtensionTemplate field = new GraphQLTypeExtensionFieldTemplate(
                schemaOptions,
                typeToExtend,
                fieldName,
                resolutionMode);

            schemaOptions.AddSchemaItemTemplate(field);
            return field;
        }

        private static IGraphQLDirectiveTemplate MapDirectiveInternal(this SchemaOptions schemaOptions, string directiveName)
        {
            var directive = new GraphQLDirectiveTemplate(schemaOptions, directiveName);

            schemaOptions.AddSchemaItemTemplate(directive);
            return directive;
        }
    }
}