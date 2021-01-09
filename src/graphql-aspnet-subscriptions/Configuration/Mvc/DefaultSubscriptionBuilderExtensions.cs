﻿// *************************************************************
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
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;
    using GraphQL.AspNet.Security;
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
            return schemaBuilder
                .AddSubscriptionServer(options)
                .AddSubscriptionPublishing();
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
            schemaBuilder.AsServiceCollection().TryAdd(CreateDefaultSubscriptionListenerServiceDescriptor());

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
            return schemaBuilder.AddSubscriptionServer<InProcessSubscriptionEventListener, TSchema>(options);
        }

        /// <summary>
        /// Adds a subscription server to this instance that will accept connected clients and
        /// process subscription requests from those clients. This extension will attempt to inject subscription related
        /// middleware into the primary query excution pipeline and replace it. Call this method before injecting or
        /// adding your own query execution middleware items.
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

            var extension = new ApolloSubscriptionServerSchemaExtension<TSchema>(schemaBuilder, subscriptionsOptions);

            // register the custom listener type to the service collection before
            // the extension can register the default
            var defaultListenerDescriptor = CreateDefaultSubscriptionListenerServiceDescriptor();
            extension.RequiredServices.Add(defaultListenerDescriptor);

            schemaBuilder.Options.RegisterExtension(extension);

            return schemaBuilder;
        }

        private static ServiceDescriptor CreateDefaultSubscriptionListenerServiceDescriptor()
        {
            return new ServiceDescriptor(
                typeof(ISubscriptionEventListener),
                (sp) =>
                        {
                            var logger = sp.CreateScope().ServiceProvider.GetService<IGraphEventLogger>();
                            return new InProcessSubscriptionEventListener(logger);
                        },
                ServiceLifetime.Singleton);
        }
    }
}