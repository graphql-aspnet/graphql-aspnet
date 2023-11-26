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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;

    /// <summary>
    /// Extension methods for defining subscriptions via minimal api.
    /// </summary>
    public static partial class GraphQLRuntimeSubscriptionDefinitionExtensions
    {
        /// <summary>
        /// Begins a new field group for the subscription schema object. All fields created using
        /// this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaBuilder">The builder to append the Subscription group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldGroupDefinition MapSubscriptionGroup(this ISchemaBuilder schemaBuilder, string template)
        {
            return MapSubscriptionGroup(schemaBuilder?.Options, template);
        }

        /// <summary>
        /// Begins a new field group for the subscription schema object. All fields created using
        /// this group will be nested underneath it and inherit any set parameters such as authorization requirements.
        /// </summary>
        /// <param name="schemaOptions">The schema options to append the Subscription group to.</param>
        /// <param name="template">The template path for this group.</param>
        /// <returns>IGraphQLRuntimeFieldDefinition.</returns>
        public static IGraphQLRuntimeFieldGroupDefinition MapSubscriptionGroup(this SchemaOptions schemaOptions, string template)
        {
            schemaOptions?.ServiceCollection?.AddSubscriptionRuntimeFieldExecutionSupport();
            return new RuntimeSubscriptionEnabledFieldGroupTemplate(schemaOptions, template);
        }
    }
}