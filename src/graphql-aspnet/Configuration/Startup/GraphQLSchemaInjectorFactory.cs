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
    using System.Linq;
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
        /// Creates a new schema injector capable of configuring the provided <paramref name="schemaOptions" />
        /// to properly serve schema dependencies. If an injector is already registered for the target
        /// schema it is returned directly.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being created.</typeparam>
        /// <param name="instance">Will be set to the instance that was located or created.</param>
        /// <param name="schemaOptions">A set of schema options to use when generating entity
        /// templates to be injected.</param>
        /// <param name="configurationDelegate">A configuration delegate to configure
        /// schema specific settings.</param>
        /// <returns><c>true</c> if this instance was successfully retrieved, <c>false</c> is the instance
        /// was newly created.</returns>
        public static bool TryGetOrCreate<TSchema>(
            out ISchemaInjector<TSchema> instance,
            SchemaOptions<TSchema> schemaOptions,
            Action<SchemaOptions<TSchema>> configurationDelegate = null)
            where TSchema : class, ISchema
        {
            instance = null;
            Validation.ThrowIfNull(schemaOptions, nameof(schemaOptions));

            var registeredInjector = schemaOptions.ServiceCollection?
                .SingleOrDefault(x => x.ImplementationInstance is ISchemaInjector<TSchema>)?
                .ImplementationInstance as ISchemaInjector<TSchema>;

            if (registeredInjector != null)
            {
                instance = registeredInjector;
                return true;
            }

            instance = Create<TSchema>(schemaOptions, configurationDelegate);
            return false;
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

        /// <summary>
        /// Creates a new schema injector capable of configuring the provided <paramref name="serviceCollection"/>
        /// to properly serve schema dependencies. If the provided service collection already contains
        /// a registered instance of the target injector it is returned directly.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema being created.</typeparam>
        /// <param name="instance">Will be set to the instance that was located or created.</param>
        /// <param name="serviceCollection">The service collection to populate.</param>
        /// <param name="configurationDelegate">A configuration delegate to configure
        /// schema specific settings.</param>
        /// <returns><c>true</c> if this instance was successfully retrieved, <c>false</c> is the instance
        /// was newly created.</returns>
        public static bool TryGetOrCreate<TSchema>(
            out ISchemaInjector<TSchema> instance,
            IServiceCollection serviceCollection,
            Action<SchemaOptions<TSchema>> configurationDelegate = null)
            where TSchema : class, ISchema
        {
            instance = null;
            Validation.ThrowIfNull(serviceCollection, nameof(serviceCollection));

            var registeredInjector = serviceCollection
                .SingleOrDefault(x => x.ImplementationInstance is ISchemaInjector<TSchema>)?
                .ImplementationInstance as ISchemaInjector<TSchema>;

            if (registeredInjector != null)
            {
                instance = registeredInjector;
                return true;
            }

            instance = Create<TSchema>(serviceCollection, configurationDelegate);
            return false;
        }
    }
}