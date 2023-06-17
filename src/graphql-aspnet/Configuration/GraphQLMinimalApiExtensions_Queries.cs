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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLMinimalApiExtensions
    {
        public static IGraphQLFieldTemplate MapQuery(this SchemaOptions schemaOptions, string template)
        {
            var field = MapGraphQLField(
                schemaOptions,
                GraphOperationType.Query,
                template);

            return field;
        }

        public static IGraphQLResolvedFieldTemplate MapQuery(this SchemaOptions schemaOptions, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var field = MapGraphQLField(
                schemaOptions,
                GraphOperationType.Query,
                template);

            return field.AddResolver(resolverMethod);
        }

        public static IGraphQLFieldTemplate MapQuery(this ISchemaBuilder schemaBuilder, string template)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            var field = MapGraphQLField(
                schemaBuilder.Options,
                GraphOperationType.Query,
                template);

            return field;
        }

        public static IGraphQLResolvedFieldTemplate MapQuery(this ISchemaBuilder schemaBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var field = MapGraphQLField(
                schemaBuilder.Options,
                GraphOperationType.Query,
                template);

            return field.AddResolver(resolverMethod);
        }
    }
}