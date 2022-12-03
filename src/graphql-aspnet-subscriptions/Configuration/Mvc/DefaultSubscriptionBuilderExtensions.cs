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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Execution.Subscriptions.BackgroundServices;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// A set of extensions to configure web socket support at startup.
    /// </summary>
    public static class DefaultSubscriptionBuilderExtensions
    {
        /// <summary>
        /// Adds the ability for this graphql server to raise and receive subscription events.
        /// Call this method before injecting or adding your own query execution middleware items.
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
            // publishing is registered AFTER the subscription server
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
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            var publishingExtension = new SubscriptionPublisherSchemaExtension<TSchema>();

            // register the internal queueing mechanism that will asyncrounously transfer
            // raised events from controller methods to the registered subscription publisher
            schemaBuilder.Options.ServiceCollection.AddSingleton<SubscriptionEventPublishingQueue>();
            schemaBuilder.Options.ServiceCollection.AddHostedService<SubscriptionPublicationService>();

            // register the in-process publisher (and related services) to the service collection before
            // if one is not already registered. The in process publisher relies on the
            // dispatch queue
            var registeredPublisherDescriptor = schemaBuilder.Options.ServiceCollection.FirstOrDefault(x => x.ServiceType == typeof(ISubscriptionEventPublisher));
            if (registeredPublisherDescriptor == null)
            {
                var defaultPublisher = CreateDefaultSubscriptionPublisherDescriptor();
                schemaBuilder.Options.ServiceCollection.TryAdd(defaultPublisher);
                schemaBuilder.Options.ServiceCollection.TryAdd(CreateDefaultSubscriptionRouterDescriptor());
            }

            schemaBuilder.Options.RegisterExtension(publishingExtension);
            schemaBuilder.QueryExecutionPipeline.AddMiddleware<PublishRaisedSubscriptionEventsMiddleware<TSchema>>(
                ServiceLifetime.Singleton);


            return schemaBuilder;
        }

        /// <summary>
        /// Adds the ability for this graphql server to receive subscription events
        /// and client connections.
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

            // try register the default router type to the service collection
            schemaBuilder.Options.ServiceCollection.TryAdd(CreateDefaultSubscriptionRouterDescriptor());

            var receiverExtension = new SubscriptionReceiverSchemaExtension<TSchema>(schemaBuilder, subscriptionsOptions);
            schemaBuilder.Options.RegisterExtension(receiverExtension);

            var validationExtension = new SubscriptionEventValidationSchemaExtension<TSchema>();
            schemaBuilder.Options.RegisterExtension(validationExtension);

            return schemaBuilder;
        }

        private static ServiceDescriptor CreateDefaultSubscriptionRouterDescriptor()
        {
            return new ServiceDescriptor(
                typeof(ISubscriptionEventRouter),
                typeof(DefaultSubscriptionEventRouter),
                ServiceLifetime.Singleton);
        }

        private static ServiceDescriptor CreateDefaultSubscriptionPublisherDescriptor()
        {
            return new ServiceDescriptor(
                 typeof(ISubscriptionEventPublisher),
                 typeof(InProcessSubscriptionPublisher),
                 ServiceLifetime.Scoped);
        }
    }
}