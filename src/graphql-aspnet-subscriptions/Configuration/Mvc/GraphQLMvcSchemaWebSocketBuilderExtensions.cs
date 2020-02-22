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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using Microsoft.Extensions.DependencyInjection;

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
        /// Adds the ability for this graphql server to raise subscription events using the default
        /// inprocess publisher.
        /// </summary>
        /// <typeparam name="TSchema">The type of schema being configured.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Configuration.ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptionPublishing<TSchema>(
                    this ISchemaBuilder<TSchema> schemaBuilder)
                    where TSchema : class, ISchema
        {
            return schemaBuilder.AddSubscriptionPublishing<InProcessSubscriptionPublisher, TSchema>();
        }

        /// <summary>
        /// Adds the ability for this graphql server to raise subscription events with a custom
        /// publisher object.
        /// </summary>
        /// <typeparam name="TPublisher">The publisher type to use when publishing events.</typeparam>
        /// <typeparam name="TSchema">The type of schema being configured.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Configuration.ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptionPublishing<TPublisher, TSchema>(
                    this ISchemaBuilder<TSchema> schemaBuilder)
                    where TPublisher : class, ISubscriptionEventPublisher
                    where TSchema : class, ISchema
        {
            var extension = new SubscriptionPublisherSchemaExtension<TSchema>();

            // register the custom publisher type to the service collection before
            // the extension can register the default
            extension.OptionalServices.Add(new ServiceDescriptor(
                 typeof(ISubscriptionEventPublisher), typeof(TPublisher), ServiceLifetime.Scoped));

            schemaBuilder.Options.RegisterExtension(extension);
            schemaBuilder.QueryExecutionPipeline.AddMiddleware<PublishRaisedSubscriptionEventsMiddleware<TSchema>>(
                ServiceLifetime.Singleton);

            schemaBuilder.AsServiceCollection().AddSingleton<SubscriptionEventQueue>();
            schemaBuilder.AsServiceCollection().AddHostedService<SubscriptionPublicationService>();

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
            return schemaBuilder.AddSubscriptionServer<InProcessSubscriptionEventListener, TSchema>();
        }

        /// <summary>
        /// Adds a subscription server to this instance that will accept connected clients and
        /// process subscription requests from those clients.
        /// </summary>
        /// <typeparam name="TListener">The type of the event listener to use on this subscription server.</typeparam>
        /// <typeparam name="TSchema">The type of the schema being built.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <param name="options">An action function to configure the subscription options.</param>
        /// <returns>ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptionServer<TListener, TSchema>(
            this ISchemaBuilder<TSchema> schemaBuilder,
            Action<SubscriptionServerOptions<TSchema>> options = null)
            where TListener : class, ISubscriptionEventListener
            where TSchema : class, ISchema
        {
            var subscriptionsOptions = new SubscriptionServerOptions<TSchema>();
            options?.Invoke(subscriptionsOptions);

            var extension = new ApolloSubscriptionServerSchemaExtension<TSchema>(subscriptionsOptions);

            // register the custom listener type to the service collection before
            // the extension can register the default
            extension.RequiredServices.Add(new ServiceDescriptor(
                 typeof(ISubscriptionEventListener), typeof(TListener), ServiceLifetime.Singleton));

            schemaBuilder.Options.RegisterExtension(extension);

            return schemaBuilder;
        }
    }
}