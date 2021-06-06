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
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// A set of extensions to configure web socket support at startup.
    /// </summary>
    public static class DefaultSubscriptionBuilderExtensions
    {
        /// <summary>
        /// Adds the ability for this graphql server to raise subscription events as well
        /// as creates a subscription server that can accept connected clients and respond to subscription events. This extension will attempt to inject subscription related
        /// middleware into the primary query excution pipeline and replace it. Call this method before injecting or
        /// adding your own query execution middleware items.
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
            // publsihing is registered AFTER the subscription server
            // because subscription server rebuilds the query execution pipeline
            // then publishing adds one additional middleware component
            return schemaBuilder
                .AddSubscriptionServer(options)
                .AddSubscriptionPublishing();
        }

        /// <summary>
        /// Adds the ability for this graphql server to raise subscription events that can be published.
        /// </summary>
        /// <typeparam name="TSchema">The type of schema being configured.</typeparam>
        /// <param name="schemaBuilder">The schema builder.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Configuration.ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddSubscriptionPublishing<TSchema>(
                    this ISchemaBuilder<TSchema> schemaBuilder)
                    where TSchema : class, ISchema
        {
            var extension = new SubscriptionPublisherSchemaExtension<TSchema>();

            // register the in-process publisher to the service collection before
            // if one is not already registered
            var defaultPublisher = CreateDefaultSubscriptionPublisherDescriptor();
            extension.OptionalServices.Add(defaultPublisher);

            // register the internal queueing mechanism that will asyncrounously transfer
            // raised events from controller methods to the registered subscription publisher
            schemaBuilder.AsServiceCollection().AddSingleton<SubscriptionEventQueue>();
            schemaBuilder.AsServiceCollection().AddHostedService<SubscriptionPublicationService>();
            schemaBuilder.AsServiceCollection().TryAdd(CreateDefaultSubscriptionRouterDescriptor());

            schemaBuilder.Options.RegisterExtension(extension);
            schemaBuilder.QueryExecutionPipeline.AddMiddleware<PublishRaisedSubscriptionEventsMiddleware<TSchema>>(
                ServiceLifetime.Singleton);

            return schemaBuilder;
        }

        /// <summary>
        /// Adds a subscription server to this instance that will accept connected clients and
        /// process subscription requests from those clients. This extension will attempt to inject subscription related
        /// middleware into the primary query excution pipeline and replace it. Call this method before injecting or
        /// adding your own query execution middleware items.
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

            var extension = new ApolloSubscriptionServerSchemaExtension<TSchema>(schemaBuilder, subscriptionsOptions);

            // register the default router type to the service collection
            var defaultRouter = CreateDefaultSubscriptionRouterDescriptor();
            extension.OptionalServices.Add(defaultRouter);

            schemaBuilder.Options.RegisterExtension(extension);

            return schemaBuilder;
        }

        private static ServiceDescriptor CreateDefaultSubscriptionRouterDescriptor()
        {
            return new ServiceDescriptor(
                typeof(ISubscriptionEventRouter),
                (sp) =>
                        {
                            var logger = sp.CreateScope().ServiceProvider.GetService<IGraphEventLogger>();
                            return new DefaultSubscriptionEventRouter(logger);
                        },
                ServiceLifetime.Singleton);
        }

        private static ServiceDescriptor CreateDefaultSubscriptionPublisherDescriptor()
        {
            return new ServiceDescriptor(
                 typeof(ISubscriptionEventPublisher), typeof(InProcessSubscriptionPublisher), ServiceLifetime.Scoped);
        }
    }
}