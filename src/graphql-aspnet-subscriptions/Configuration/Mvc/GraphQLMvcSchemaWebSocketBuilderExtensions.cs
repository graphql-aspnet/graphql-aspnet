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
    using System.Runtime.CompilerServices;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of extensions to configure web socket support at startup.
    /// </summary>
    public static class GraphQLMvcSchemaWebSocketBuilderExtensions
    {
        /// <summary>
        /// Adds the ability for this graphql server to raise subscription events as well
        /// as creates a subscription server that can accept connected clients and respond to subscription events.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being built.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <param name="options">An action function to configure the subscription options.</param>
        /// <returns>ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptions<TSchema>(
            this ISchemaBuilder<TSchema> schemaBuilder,
            Action<SubscriptionServerOptions<TSchema>> options = null)
                    where TSchema : class, ISchema
        {
            return schemaBuilder
                .AddSubscriptionPublishing()
                .AddSubscriptionServer(options);
        }

        /// <summary>
        /// Adds the ability for this graphql server to raise subscription events.
        /// </summary>
        /// <typeparam name="TSchema">The type of schema being configured.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Configuration.ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptionPublishing<TSchema>(
                    this ISchemaBuilder<TSchema> schemaBuilder)
                    where TSchema : class, ISchema
        {
            var extension = new SubscriptionPublisherExtension<TSchema>();
            schemaBuilder.Options.RegisterExtension(extension);

            return schemaBuilder;
        }

        /// <summary>
        /// Adds a subscription server to this instance that will accept connected clients and
        /// process subscription requests from those clients.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being built.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <param name="options">An action function to configure the subscription options.</param>
        /// <returns>ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptionServer<TSchema>(
            this ISchemaBuilder<TSchema> schemaBuilder,
            Action<SubscriptionServerOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            var subscriptionsOptions = new SubscriptionServerOptions<TSchema>();
            options?.Invoke(subscriptionsOptions);

            var extension = new SubscriptionServerExtension<TSchema>(subscriptionsOptions);
            schemaBuilder.Options.RegisterExtension(extension);

            return schemaBuilder;
        }
    }
}