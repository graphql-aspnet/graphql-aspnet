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
    using GraphQL.AspNet.Configuration.MinimalApi;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static class GraphQLMinimalApiExtensions
    {
        public static IGraphQLFieldBuilder MapQuery(this SchemaOptions schemaOptions, string template)
        {
            return MapGraphQLField(
                schemaOptions,
                GraphOperationType.Query,
                template);
        }

        public static IGraphQLFieldBuilder MapMutation(this SchemaOptions schemaOptions, string template)
        {
            return MapGraphQLField(
                schemaOptions,
                GraphOperationType.Mutation,
                template);
        }

        public static IGraphQLFieldBuilder MapQuery(this ISchemaBuilder schemaBuilder, string template)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            return MapGraphQLField(
                schemaBuilder.Options,
                GraphOperationType.Query,
                template);
        }

        public static IGraphQLFieldBuilder MapMutation(this ISchemaBuilder schemaBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            return MapGraphQLField(
                schemaBuilder.Options,
                GraphOperationType.Mutation,
                template);
        }

        public static IGraphQLFieldBuilder MapQuery(this IGraphQLFieldGroupBuilder groupBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(groupBuilder, nameof(groupBuilder));

            return MapGraphQLField(
                groupBuilder.Options,
                GraphOperationType.Query,
                template);
        }

        public static IGraphQLFieldBuilder MapMutation(this IGraphQLFieldGroupBuilder groupBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(groupBuilder, nameof(groupBuilder));

            return MapGraphQLField(
                groupBuilder.Options,
                GraphOperationType.Mutation,
                template);
        }

        public static IGraphQLFieldGroupBuilder MapGroup(this ISchemaBuilder schemaBuilder, string template)
        {
            return MapGroup(schemaBuilder?.Options, template);
        }

        public static IGraphQLFieldGroupBuilder MapGroup(this SchemaOptions schemaOptions, string template)
        {
            return new GraphQLFieldGroupBuilder(schemaOptions, template);
        }

        public static IGraphQLFieldGroupBuilder MapGroup(this IGraphQLFieldGroupBuilder groupBuilder, string subTemplate)
        {
            return new GraphQLFieldGroupBuilder(groupBuilder, subTemplate);
        }

        public static IGraphQLFieldBuilder MapGraphQLField(
            SchemaOptions schemaOptions,
            GraphOperationType operationType,
            string pathTemplate)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            pathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(pathTemplate, nameof(pathTemplate));

            var path = new SchemaItemPath((SchemaItemCollections)operationType, pathTemplate);

            var fieldTemplate = new GraphQLFieldBuilder(
                schemaOptions,
                path.Path);

            schemaOptions.AddFieldTemplate(fieldTemplate);
            return fieldTemplate;
        }
    }
}