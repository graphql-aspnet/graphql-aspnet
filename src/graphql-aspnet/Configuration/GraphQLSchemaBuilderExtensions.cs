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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration.Startup;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Helper methods for wiring up a a graph QL schema to a service collection.
    /// </summary>
    public static class GraphQLSchemaBuilderExtensions
    {
        private static readonly Dictionary<IServiceCollection, ISchemaInjectorCollection> SCHEMA_REGISTRATIONS;

        /// <summary>
        /// Initializes static members of the <see cref="GraphQLSchemaBuilderExtensions"/> class.
        /// </summary>
        static GraphQLSchemaBuilderExtensions()
        {
            SCHEMA_REGISTRATIONS = new Dictionary<IServiceCollection, ISchemaInjectorCollection>();
        }

        /// <summary>
        /// Helper method to null out the schema registration references. Useful in testing and after setup is complete there is no
        /// need to keep the reference chain in tact.
        /// </summary>
        /// <param name="serviceCollection">The service collection to clear.</param>
        public static void Clear(IServiceCollection serviceCollection)
        {
            lock (SCHEMA_REGISTRATIONS)
            {
                if (!SCHEMA_REGISTRATIONS.TryGetValue(serviceCollection, out var value))
                {
                    return;
                }

                value.Clear();
                SCHEMA_REGISTRATIONS.Remove(serviceCollection);
            }
        }

        /// <summary>
        /// Helper method to null out all schema injector collections. Useful in testing and after setup is complete there is no
        /// need to keep the reference chain in tact.
        /// </summary>
        public static void Clear()
        {
            lock (SCHEMA_REGISTRATIONS)
            {
                foreach (var injectorCollection in SCHEMA_REGISTRATIONS.Values)
                {
                    injectorCollection.Clear();
                }

                SCHEMA_REGISTRATIONS.Clear();
            }
        }

        /// <summary>
        /// Enables the query cache locally, in memory, to retain parsed query plans. When enabled, use the configuration
        /// settings for each added schema to determine how each will interact with the cache. Implement your own cache provider
        /// by inheriting from <see cref="IQueryExecutionPlanCacheProvider" /> and registering it to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add the local cache to.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddGraphQLLocalQueryCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IQueryExecutionPlanCacheProvider, DefaultQueryExecutionPlanCacheProvider>();
            serviceCollection.AddSingleton<IQueryExecutionPlanCacheKeyManager, DefaultQueryExecutionPlanCacheKeyManager>();
            return serviceCollection;
        }

        /// <summary>
        /// Adds the typed graph schema to the application.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="options">An action to configure or add additional options to the schema
        /// as its being built.</param>
        /// <returns>ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddGraphQL<TSchema>(
            this IServiceCollection serviceCollection,
            Action<SchemaOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(serviceCollection, nameof(serviceCollection));
            var injectorCollection = GetOrAddSchemaInjectorCollection(serviceCollection);
            if (injectorCollection.ContainsKey(typeof(TSchema)))
            {
                throw new GraphTypeDeclarationException(
                    $"The schema type {typeof(TSchema).FriendlyName()} has already been registered. " +
                    "Each schema type may only be registered once with GraphQL.");
            }

            var schemaOptions = new SchemaOptions<TSchema>(serviceCollection);
            var injector = new GraphQLSchemaInjector<TSchema>(schemaOptions, options);
            injectorCollection.Add(typeof(TSchema), injector);

            injector.ConfigureServices();
            return injector.SchemaBuilder;
        }

        /// <summary>
        /// Adds a singular graph schema to the application.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="options">An action to configure or add additional options to the schema as its being built.</param>
        /// <returns>ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<GraphSchema> AddGraphQL(
            this IServiceCollection serviceCollection,
            Action<SchemaOptions<GraphSchema>> options = null)
        {
            return AddGraphQL<GraphSchema>(serviceCollection, options);
        }

        /// <summary>
        /// Performs final configuration on graphql, preparses any referenced types for their meta data,
        /// compiles the schema instance and sets up the query handler to accept web requests
        /// at this point in the ASP.NET request pipeline.
        /// </summary>
        /// <param name="app">The application being constructed.</param>
        public static void UseGraphQL(this IApplicationBuilder app)
        {
            var injectorCollection = app.ApplicationServices.GetRequiredService<ISchemaInjectorCollection>();
            foreach (var injector in injectorCollection.Values)
            {
                injector.UseSchema(app);
            }

            Clear(injectorCollection.ServiceCollection);
        }

        /// <summary>
        /// <para>
        /// Performs final configuration on graphql, preparses any referenced types for their meta data,
        /// registers the schema with the runtime.
        /// </para>
        /// <para>
        /// NOTE: No query handler will be registered with ASP.NET and requests will NOT
        /// be configured to be served via HTTP when this method is called via the <see cref="IServiceProvider"/>.
        /// </para>
        /// </summary>
        /// <param name="serviceProvider">The service provider from which to construct and configure the
        /// graphql runtime.</param>
        public static void UseGraphQL(this IServiceProvider serviceProvider)
        {
            var injectorCollection = serviceProvider.GetRequiredService<ISchemaInjectorCollection>();
            foreach (var injector in injectorCollection.Values)
            {
                injector.UseSchema(serviceProvider);
            }

            Clear(injectorCollection.ServiceCollection);
        }

        /// <summary>
        /// Get or create a schema injector collection for the service collection being harnessed.
        /// </summary>
        /// <param name="serviceCollection">The service collection to create schema injector collection for</param>
        /// <returns>Existing or new schema injector collection</returns>
        private static ISchemaInjectorCollection GetOrAddSchemaInjectorCollection(IServiceCollection serviceCollection)
        {
            if (SCHEMA_REGISTRATIONS.TryGetValue(serviceCollection, out var value))
                return value;

            lock (SCHEMA_REGISTRATIONS)
            {
                if (SCHEMA_REGISTRATIONS.TryGetValue(serviceCollection, out value))
                    return value;

                var injectorCollection = new SchemaInjectorCollection(serviceCollection);

                serviceCollection.AddSingleton<ISchemaInjectorCollection>(injectorCollection);
                value = injectorCollection;
                SCHEMA_REGISTRATIONS.Add(serviceCollection, value);
            }

            return value;
        }
    }
}