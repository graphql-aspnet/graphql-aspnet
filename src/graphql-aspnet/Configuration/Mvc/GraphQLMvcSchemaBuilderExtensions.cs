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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Helper methods for wiring up a a graph QL schema to a service collection.
    /// </summary>
    public static class GraphQLMvcSchemaBuilderExtensions
    {
        private static readonly Dictionary<Type, ISchemaInjector> SCHEMA_REGISTRATIONS;

        /// <summary>
        /// Initializes static members of the <see cref="GraphQLMvcSchemaBuilderExtensions"/> class.
        /// </summary>
        static GraphQLMvcSchemaBuilderExtensions()
        {
            SCHEMA_REGISTRATIONS = new Dictionary<Type, ISchemaInjector>();
        }

        /// <summary>
        /// Helper method to null out the schema registration references. Useful in testing and after setup is complete there is no
        /// need to keep the reference chain in tact.
        /// </summary>
        public static void Clear()
        {
            SCHEMA_REGISTRATIONS.Clear();
        }

        /// <summary>
        /// Enables the query cache locally, in memory, to retain parsed query plans. When enabled, use the configuration
        /// settings for each added schema to determine how each will interact with the cache. Implement your own cache provider
        /// by inheriting from <see cref="IGraphQueryPlanCacheProvider" /> and registering it to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add the local cache to.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddGraphQLLocalQueryCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IGraphQueryPlanCacheProvider, DefaultQueryPlanCacheProvider>();
            serviceCollection.AddSingleton<IGraphQueryPlanCacheKeyManager, DefaultQueryPlanCacheKeyManager>();
            return serviceCollection;
        }

        /// <summary>
        /// Adds the typed graph schema to the application.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="options">An action to configure or add additional options to the schema
        /// as its being built.</param>
        /// <returns>GraphQLServiceCollectionBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<TSchema> AddGraphQL<TSchema>(
            this IServiceCollection serviceCollection,
            Action<SchemaOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(serviceCollection, nameof(serviceCollection));
            if (SCHEMA_REGISTRATIONS.ContainsKey(typeof(TSchema)))
            {
                throw new GraphTypeDeclarationException(
                    $"A schema type {typeof(TSchema).FriendlyName()} has already been registered. " +
                    "Eac schema type may only be registered once with GraphQL.");
            }

            var schemaOptions = new SchemaOptions<TSchema>(serviceCollection);
            var injector = new GraphQLSchemaInjector<TSchema>(schemaOptions, options);
            SCHEMA_REGISTRATIONS.Add(typeof(TSchema), injector);

            injector.ConfigureServices();
            return injector.SchemaBuilder;
        }

        /// <summary>
        /// Adds a singular graph schema to the application.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="options">An action to configure or add additional options to the schema as its being built.</param>
        /// <returns>GraphQLServiceCollectionBuilder&lt;TSchema&gt;.</returns>
        public static ISchemaBuilder<GraphSchema> AddGraphQL(
            this IServiceCollection serviceCollection,
            Action<SchemaOptions<GraphSchema>> options = null)
        {
            return AddGraphQL<GraphSchema>(serviceCollection, options);
        }

        /// <summary>
        /// Performs final configuration on graphql and preparses any referenced types for their meta data.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void UseGraphQL(this IApplicationBuilder app)
        {
            foreach (var injector in SCHEMA_REGISTRATIONS.Values)
            {
                injector.UseSchema(app);
            }

            Clear();
        }

        /// <summary>
        /// Performs final configuration on graphql and preparses any referenced types for their meta data.
        /// Will NOT attempt to register an HTTP for the schema.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void UseGraphQL(this IServiceProvider serviceProvider)
        {
            foreach (var injector in SCHEMA_REGISTRATIONS.Values)
            {
                injector.UseSchema(serviceProvider);
            }

            Clear();
        }
    }
}