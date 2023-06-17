﻿// *************************************************************
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
    using GraphQL.AspNet.Configuration.MinimalApi;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLMinimalApiExtensions
    {
        private static IGraphQLFieldTemplate MapGraphQLField(
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

        private static IGraphQLResolvedFieldTemplate MapTypeExtension(
            SchemaOptions schemaOptions,
            Type typeToExtend,
            string fieldName,
            FieldResolutionMode resolutionMode)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            fieldName = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));

            IGraphQLResolvedFieldTemplate field = new GraphQLTypeExtensionFieldTemplate(
                schemaOptions,
                typeToExtend,
                fieldName,
                resolutionMode);

            schemaOptions.AddFieldTemplate(field);
            return field;
        }
    }
}