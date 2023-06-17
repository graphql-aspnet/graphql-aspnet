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
        public static IGraphQLFieldTemplate MapMutation(this SchemaOptions schemaOptions, string template)
        {
            var field = MapGraphQLField(
                schemaOptions,
                GraphOperationType.Mutation,
                template);

            return field;
        }

        public static IGraphQLResolvedFieldTemplate MapMutation(this SchemaOptions schemaOptions, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var field = MapGraphQLField(
                schemaOptions,
                GraphOperationType.Mutation,
                template);

            return field.AddResolver(resolverMethod);
        }

        public static IGraphQLFieldTemplate MapMutation(this ISchemaBuilder schemaBuilder, string template)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            var field = MapGraphQLField(
                schemaBuilder.Options,
                GraphOperationType.Mutation,
                template);

            return field;
        }

        public static IGraphQLResolvedFieldTemplate MapMutation(this ISchemaBuilder schemaBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var field = MapGraphQLField(
                schemaBuilder.Options,
                GraphOperationType.Mutation,
                template);

            return field.AddResolver(resolverMethod);
        }
    }
}