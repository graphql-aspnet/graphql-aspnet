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
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLMinimalApiExtensions
    {
        /// <summary>
        /// Creates a new field in the query root object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLFieldTemplate MapQuery(this SchemaOptions schemaOptions, string template)
        {
            var field = MapGraphQLFieldInternal(
                schemaOptions,
                GraphOperationType.Query,
                template);

            return field;
        }

        /// <summary>
        /// Creates a new, explicitly resolvable field in the query root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="resolverMethod">The resolver method to execute when
        /// this field is requested by a caller.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate MapQuery(this SchemaOptions schemaOptions, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var field = MapGraphQLFieldInternal(
                schemaOptions,
                GraphOperationType.Query,
                template);

            return field.AddResolver(resolverMethod);
        }

        /// <summary>
        /// Creates a new, explicitly resolvable field in the query root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLFieldTemplate MapQuery(this ISchemaBuilder schemaBuilder, string template)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            var field = MapGraphQLFieldInternal(
                schemaBuilder.Options,
                GraphOperationType.Query,
                template);

            return field;
        }

        /// <summary>
        /// Creates a new field in the query object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="resolverMethod">The resolver method to execute when this
        /// field is requested.</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate MapQuery(this ISchemaBuilder schemaBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var field = MapGraphQLFieldInternal(
                schemaBuilder.Options,
                GraphOperationType.Query,
                template);

            return field.AddResolver(resolverMethod);
        }
    }
}