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

    /// <summary>
    /// Extension methods for defining subscriptions via minimal api.
    /// </summary>
    public static partial class GraphQLRuntimeSubscriptionDefinitionExtensions
    {
        /// <summary>
        /// Creates a new, explicitly resolvable field in the Subscription root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition MapSubscription(this ISchemaBuilder schemaBuilder, string template)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            return MapSubscription(
                schemaBuilder.Options,
                template,
                null, // unionName
                null as Delegate);
        }

        /// <summary>
        /// Creates a new field in the Subscription object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="resolverMethod">The resolver method to execute when this
        /// field is requested.</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition MapSubscription(this ISchemaBuilder schemaBuilder, string template, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            return MapSubscription(
                schemaBuilder.Options,
                template,
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Creates a new field in the Subscription object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="GraphQLRuntimeSchemaItemDefinitionExtensions.AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to execute when this
        /// field is requested.</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition MapSubscription(
            this ISchemaBuilder schemaBuilder,
            string template,
            string unionName,
            Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            return MapSubscription(
                schemaBuilder.Options,
                template,
                unionName,
                resolverMethod);
        }

        /// <summary>
        /// Creates a new field in the Subscription root object with the given path. This field can act as a
        /// grouping field of other resolvable fields or be converted to an explicitly resolvable field itself.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <returns>IGraphQLFieldTemplate.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition MapSubscription(
            this SchemaOptions schemaOptions,
            string template)
        {
            return MapSubscription(
                schemaOptions,
                template,
                null, // unionMethod
                null as Delegate);
        }

        /// <summary>
        /// Creates a new, explicitly resolvable field in the Subscription root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="resolverMethod">The resolver method to execute when
        /// this field is requested by a caller.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition MapSubscription(
            this SchemaOptions schemaOptions,
            string template,
            Delegate resolverMethod)
        {
            return MapSubscription(
                schemaOptions,
                template,
                null, // unionMethod
                resolverMethod);
        }

        /// <summary>
        /// Creates a new, explicitly resolvable field in the Subscription root object with the given path. This field cannot be
        /// further extended or nested with other fields via the Mapping API.
        /// </summary>
        /// <param name="schemaOptions">The options representing the schema where this field
        /// will be created.</param>
        /// <param name="template">The template path string for his field. (e.g. <c>/path1/path2/path3</c>)</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="GraphQLRuntimeSchemaItemDefinitionExtensions.AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to execute when
        /// this field is requested by a caller.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition MapSubscription(
            this SchemaOptions schemaOptions,
            string template,
            string unionName,
            Delegate resolverMethod)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            template = Validation.ThrowIfNullWhiteSpaceOrReturn(template, nameof(template));

            var fieldTemplate = new RuntimeSubscriptionEnabledFieldGroupTemplate(
                schemaOptions,
                template);

            var resolvedField = RuntimeSubscriptionEnabledResolvedFieldDefinition.FromFieldTemplate(fieldTemplate);
            schemaOptions.AddRuntimeSchemaItem(resolvedField);

            if (!string.IsNullOrWhiteSpace(unionName))
                resolvedField.AddAttribute(new UnionAttribute(unionName.Trim()));

            resolvedField.AddResolver(unionName, resolverMethod);

            schemaOptions.ServiceCollection?.AddSubscriptionRuntimeFieldExecutionSupport();
            return resolvedField;
        }

        /// <summary>
        /// Defines a custom event name for this subscription field. This event name is the key value
        /// that must be published from a mutation to invoke this subscription for any subscribers.
        /// </summary>
        /// <remarks>
        /// This event name must be unique for this schema.
        /// </remarks>
        /// <param name="field">The field to update.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <returns>IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition.</returns>
        public static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition WithEventName(
            this IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition field,
            string eventName)
        {
            field.EventName = eventName;
            return field;
        }
    }
}