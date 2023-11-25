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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLRuntimeSchemaItemDefinitionExtensions
    {
        /// <summary>
        /// Begins a new field group for the query schema object. All fields created using
        /// this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaBuilder">The builder to append the query group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldGroupDefinition MapQueryGroup(this ISchemaBuilder schemaBuilder, string template)
        {
            return MapQueryGroup(schemaBuilder?.Options, template);
        }

        /// <summary>
        /// Begins a new field group for the query schema object. All fields created using
        /// this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaOptions">The schema options to append the query group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldGroupDefinition MapQueryGroup(this SchemaOptions schemaOptions, string template)
        {
            return new RuntimeFieldGroupTemplate(schemaOptions, ItemPathRoots.Query, template);
        }

        /// <summary>
        /// Creates a new, explicitly resolvable field in the query root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapQuery(this ISchemaBuilder schemaBuilder, string template)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            return MapQuery(
                schemaBuilder.Options,
                template,
                null, // unionName
                null as Delegate);
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
        public static IGraphQLRuntimeResolvedFieldDefinition MapQuery(this ISchemaBuilder schemaBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            return MapQuery(
                schemaBuilder.Options,
                template,
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Creates a new field in the query object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to execute when this
        /// field is requested.</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapQuery(this ISchemaBuilder schemaBuilder, string template, string unionName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            return MapQuery(
                schemaBuilder.Options,
                template,
                unionName,
                resolverMethod);
        }

        /// <summary>
        /// Creates a new field in the query root object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapQuery(this SchemaOptions schemaOptions, string template)
        {
            return MapQuery(
                schemaOptions,
                template,
                null, // unionMethod
                null as Delegate);
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
        public static IGraphQLRuntimeResolvedFieldDefinition MapQuery(this SchemaOptions schemaOptions, string template, Delegate resolverMethod)
        {
            return MapQuery(
                schemaOptions,
                template,
                null, // unionMethod
                resolverMethod);
        }

        /// <summary>
        /// Creates a new, explicitly resolvable field in the query root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to execute when
        /// this field is requested by a caller.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapQuery(this SchemaOptions schemaOptions, string template, string unionName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaOptions, nameof(schemaOptions));

            var field = MapGraphQLFieldInternal(
                schemaOptions,
                GraphOperationType.Query,
                template);

            var resolvedField = RuntimeResolvedFieldDefinition.FromFieldTemplate(field);
            schemaOptions.AddRuntimeSchemaItem(resolvedField);

            if (!string.IsNullOrWhiteSpace(unionName))
                resolvedField.AddAttribute(new UnionAttribute(unionName.Trim()));

            return resolvedField.AddResolver(unionName, resolverMethod);
        }
    }
}