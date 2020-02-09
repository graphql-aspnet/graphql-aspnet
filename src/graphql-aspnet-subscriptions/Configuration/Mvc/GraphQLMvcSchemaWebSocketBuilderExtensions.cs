// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Mvc
{
    using System;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of extensions to configure web socket support at startup.
    /// </summary>
    public static class GraphQLMvcSchemaWebSocketBuilderExtensions
    {
        /// <summary>
        /// Adds subscription support to this schema.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being built.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <param name="action">The action.</param>
        /// <returns>ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptions<TSchema>(
            this ISchemaBuilder<TSchema> schemaBuilder,
            Action<SchemaSubscriptionOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            var subscriptionsOptions = new SchemaSubscriptionOptions<TSchema>();
            options?.Invoke(subscriptionsOptions);

            var extension = new SchemaSubscriptionsExtension<TSchema>(subscriptionsOptions);

            schemaBuilder.Options.RegisterExtension(extension);
            return schemaBuilder;
        }
    }
}