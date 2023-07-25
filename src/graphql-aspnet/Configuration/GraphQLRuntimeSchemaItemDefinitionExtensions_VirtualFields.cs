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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLRuntimeSchemaItemDefinitionExtensions
    {
        /// <summary>
        /// Adds policy-based authorization requirements to the field.
        /// </summary>
        /// <remarks>
        /// This is similar to adding the <see cref="AuthorizeAttribute"/> to a controller method. Subsequent calls to this
        /// method will cause more authorization restrictions to be added to the field.
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="policyName">The name of the policy to assign via this requirement.</param>
        /// <param name="roles">A comma-seperated list of roles to assign via this requirement.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeFieldDefinition RequireAuthorization(
            this IGraphQLRuntimeFieldDefinition fieldBuilder,
            string policyName = null,
            string roles = null)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));

            var attrib = new AuthorizeAttribute();
            attrib.Policy = policyName?.Trim();
            attrib.Roles = roles?.Trim();
            fieldBuilder.AddAttribute(attrib);
            return fieldBuilder;
        }

        /// <summary>
        /// Indicates that the field should allow anonymous access.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is similar to adding the <see cref="AllowAnonymousAttribute"/> to a controller method
        /// </para>
        /// <para>
        /// Any inherited authorization permissions from field groups are automatically
        /// dropped from this field instance.
        /// </para>
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeFieldDefinition AllowAnonymous(this IGraphQLRuntimeFieldDefinition fieldBuilder)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));
            if (!fieldBuilder.Attributes.OfType<AllowAnonymousAttribute>().Any())
                fieldBuilder.AddAttribute(new AllowAnonymousAttribute());

            return fieldBuilder;
        }

        /// <summary>
        /// Begins a new field group for the query schema object. All fields created using
        /// this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaBuilder">The builder to append the query group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldDefinition MapQueryGroup(this ISchemaBuilder schemaBuilder, string template)
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
        public static IGraphQLRuntimeFieldDefinition MapQueryGroup(this SchemaOptions schemaOptions, string template)
        {
            return new RuntimeVirtualFieldTemplate(schemaOptions, SchemaItemCollections.Query, template);
        }

        /// <summary>
        /// Begins a new field group for the mutation schema object. All fields created using this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaBuilder">The builder to append the mutation group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldDefinition MapMutationGroup(this ISchemaBuilder schemaBuilder, string template)
        {
            return MapMutationGroup(schemaBuilder?.Options, template);
        }

        /// <summary>
        /// Begins a new field group for the mutation schema object. All fields created using this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaOptions">The schema options to append the mutation group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldDefinition MapMutationGroup(this SchemaOptions schemaOptions, string template)
        {
            return new RuntimeVirtualFieldTemplate(schemaOptions, SchemaItemCollections.Mutation, template);
        }

        /// <summary>
        /// Maps an intermediate child field into the schema. This field will inherit any parameters set from its parent (including the template path).
        /// Any fields created from this new group will inherit any parameters set on the group AND the parent group.
        /// </summary>
        /// <remarks>
        /// Any intermediate child fields created via this method will retain their operation association (e.g. Mutation, Query etc.), it cannot be changed.
        /// </remarks>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldDefinition MapChildGroup(this IGraphQLRuntimeFieldDefinition field, string subTemplate)
        {
            return new RuntimeVirtualFieldTemplate(field, subTemplate);
        }

        /// <summary>
        /// Maps a terminal child field into the schema and assigns the resolver method to it.
        /// </summary>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <returns>IGraphQLResolvedFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapField(this IGraphQLRuntimeFieldDefinition field, string subTemplate)
        {
            return MapField(
                field,
                subTemplate,
                null as Type, // expectedReturnType
                null, // unionName
                null as Delegate);
        }

        /// <summary>
        /// Maps a terminal child field into the schema and assigns the resolver method to it.
        /// </summary>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when this field is requested.</param>
        /// <returns>IGraphQLResolvedFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapField(this IGraphQLRuntimeFieldDefinition field, string subTemplate, Delegate resolverMethod)
        {
            return MapField(
                field,
                subTemplate,
                null as Type, // expectedReturnType
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Maps a terminal child field into the schema and assigns the resolver method to it.
        /// </summary>
        /// <typeparam name="TReturnType">The expected, primary return type of the field. Must be provided
        /// if the supplied delegate returns an <see cref="IGraphActionResult"/>.</typeparam>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when this field is requested.</param>
        /// <returns>IGraphQLResolvedFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapField<TReturnType>(this IGraphQLRuntimeFieldDefinition field, string subTemplate, Delegate resolverMethod)
        {
            return MapField(
                field,
                subTemplate,
                typeof(TReturnType), // expectedReturnType
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Maps a terminal child field into the schema and assigns the resolver method to it.
        /// </summary>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to be called when this field is requested.</param>
        /// <returns>IGraphQLResolvedFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition MapField(this IGraphQLRuntimeFieldDefinition field, string subTemplate, string unionName, Delegate resolverMethod)
        {
            return MapField(
                field,
                subTemplate,
                null as Type, // expectedReturnType
                unionName, // unionName
                resolverMethod);
        }

        private static IGraphQLRuntimeResolvedFieldDefinition MapField(
            IGraphQLRuntimeFieldDefinition field,
            string subTemplate,
            Type expectedReturnType,
            string unionName,
            Delegate resolverMethod)
        {
            var subField = new RuntimeResolvedFieldDefinition(field, subTemplate);
            subField.Options.AddRuntimeSchemaItem(subField);

            subField.AddResolver(expectedReturnType, unionName, resolverMethod);
            return subField;
        }
    }
}