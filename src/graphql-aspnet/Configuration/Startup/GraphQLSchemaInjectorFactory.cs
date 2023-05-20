// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Startup
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for creating a default implementation of a <see cref="ISchemaInjector"/>.
    /// </summary>
    public static class GraphQLSchemaInjectorFactory
    {
        /// <summary>
        /// Creates a new schema injector capable of configuring the provided <paramref name="schemaOptions"/>
        /// to properly serve schema dependencies.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being created.</typeparam>
        /// <param name="schemaOptions">A set of schema options to use when generating entity
        /// templates to be injected.</param>
        /// <param name="configurationDelegate">A configuration delegate to configure
        /// schema specific settings.</param>
        /// <returns>ISchemaInjector&lt;TSchema&gt;.</returns>
        public static ISchemaInjector<TSchema> Create<TSchema>(
            SchemaOptions<TSchema> schemaOptions,
            Action<SchemaOptions<TSchema>> configurationDelegate = null)
            where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(schemaOptions, nameof(schemaOptions));

            var injector = new GraphQLSchemaInjector<TSchema>(schemaOptions, configurationDelegate);
            return injector;
        }

        /// <summary>
        /// Creates a new schema injector capable of configuring the provided <paramref name="serviceCollection"/>
        /// to properly serve schema dependencies.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being created.</typeparam>
        /// <param name="serviceCollection">The service collection to populate.</param>
        /// <param name="configurationDelegate">A configuration delegate to configure
        /// schema specific settings.</param>
        /// <returns>ISchemaInjector&lt;TSchema&gt;.</returns>
        public static ISchemaInjector<TSchema> Create<TSchema>(
            IServiceCollection serviceCollection,
            Action<SchemaOptions<TSchema>> configurationDelegate = null)
            where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(serviceCollection, nameof(serviceCollection));

            var schemaOptions = new SchemaOptions<TSchema>(serviceCollection);
            var injector = new GraphQLSchemaInjector<TSchema>(schemaOptions, configurationDelegate);
            return injector;
        }
    }
}