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
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// A complete set of configuration options to setup a schema.
    /// </summary>
    public partial class SchemaOptions
    {
        private readonly Dictionary<Type, IGraphQLServerExtension> _serverExtensions;
        private readonly HashSet<SchemaTypeToRegister> _possibleTypes;

        private readonly Type _schemaType;
        private readonly List<ServiceToRegister> _registeredServices;
        private List<ISchemaConfigurationExtension> _configExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaOptions" /> class.
        /// </summary>
        /// <param name="schemaType">Type of the schema being built.</param>
        /// <param name="serviceCollection">The service collection to which all
        /// found types or services should be registered.</param>
        public SchemaOptions(Type schemaType, IServiceCollection serviceCollection)
        {
            this.ServiceCollection = Validation.ThrowIfNullOrReturn(serviceCollection, nameof(serviceCollection));
            _schemaType = Validation.ThrowIfNullOrReturn(schemaType, nameof(schemaType));
            _possibleTypes = new HashSet<SchemaTypeToRegister>(SchemaTypeToRegister.DefaultEqualityComparer);
            _serverExtensions = new Dictionary<Type, IGraphQLServerExtension>();
            _registeredServices = new List<ServiceToRegister>();
            _configExtensions = new List<ISchemaConfigurationExtension>();

            this.DeclarationOptions = new SchemaDeclarationConfiguration();
            this.CacheOptions = new SchemaQueryPlanCacheConfiguration();
            this.AuthorizationOptions = new SchemaAuthorizationConfiguration();
            this.ExecutionOptions = new SchemaExecutionConfiguration();
            this.ResponseOptions = new SchemaResponseConfiguration();
            this.QueryHandler = new SchemaQueryHandlerConfiguration();
        }

        /// <summary>
        /// Creates the read-only, runtime configuration from the full set of options configured for the schema.
        /// </summary>
        /// <returns>ISchemaConfiguration.</returns>
        public ISchemaConfiguration CreateConfiguration()
        {
            return new SchemaConfiguration(
                this.DeclarationOptions,
                this.ExecutionOptions,
                this.ResponseOptions,
                this.CacheOptions);
        }

        /// <summary>
        /// Searches for and registers all publically accessible <see cref="GraphController" /> and  <see cref="GraphDirective"/>
        /// found on the assembly in which the current <see cref="ISchema" /> is declared, injecting them into the schema and generating the
        /// appropriate graph types.
        /// </summary>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddSchemaAssembly()
        {
            var assemblyToCheck = _schemaType.Assembly;
            return this.AddAssembly(assemblyToCheck);
        }

        /// <summary>
        /// Searches for and registers all publically accessable <see cref="GraphController" /> and <see cref="GraphDirective"/>
        /// found on the supplied assembly, injecting them into the schema as graph types.
        /// </summary>
        /// <param name="assembly">The assembly to scan for items.</param>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddAssembly(Assembly assembly)
        {
            Validation.ThrowIfNull(assembly, nameof(assembly));
            var typesToAdd = assembly.LocateTypesInAssembly(Constants.AssemblyScanTypes);
            foreach (var type in typesToAdd)
                this.AddType(type, null);

            return this;
        }

        /// <summary>
        /// Registers the type to the schema when it is instaniated, it will be made available
        /// in the type system and queriable via introspection.
        /// </summary>
        /// <typeparam name="TItem">The type to add.</typeparam>
        /// <param name="typeKind">The kind of graph type to register the type as. Standard POCOs will
        /// default to OBJECT unless otherwise specified.</param>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddGraphType<TItem>(TypeKind? typeKind = null)
        {
            if (Validation.IsCastable<GraphController>(typeof(TItem)))
            {
                throw new SchemaConfigurationException(
                    $"The type '{typeof(TItem).FriendlyName()}' cannot be registered as a graph type. It is a controller.");
            }

            if (Validation.IsCastable<GraphDirective>(typeof(TItem)))
            {
                throw new SchemaConfigurationException(
                    $"The type '{typeof(TItem).FriendlyName()}' cannot be registered as a graph type. It is a directive.");
            }

            return this.AddType(typeof(TItem), null, null);
        }

        /// <summary>
        /// Registers the controller to the schema.
        /// </summary>
        /// <typeparam name="TController">The controller to add.</typeparam>
        /// <param name="customLifetime">When supplied, the controller will be registered
        /// with the given service lifetime, otherwise the globally configured controller lifeime will be used.</param>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddController<TController>(ServiceLifetime? customLifetime = null)
            where TController : GraphController
        {
            return this.AddType(typeof(TController), null, customLifetime);
        }

        /// <summary>
        /// Registers the directive to the schema.
        /// </summary>
        /// <typeparam name="TDirective">The directive to add.</typeparam>
        /// <param name="customLifetime">When supplied, the directive will be registered
        /// with the given service lifetime, otherwise the globally configured controller lifeime will be used.</param>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddDirective<TDirective>(ServiceLifetime? customLifetime = null)
            where TDirective : GraphDirective
        {
            return this.AddType(typeof(TDirective), null, customLifetime);
        }

        /// <summary>
        /// Registers the given type to the schema. It will be made available
        /// in the type system and queriable via introspection. This method can be used to register
        /// graph types, controllers and directives.
        /// </summary>
        /// <param name="type">The type to add.</param>
        /// <param name="typeKind">The kind of graph type to register the type as. Standard POCOs will
        /// default to OBJECT unless otherwise specified.</param>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddType(Type type, TypeKind? typeKind = null)
        {
            return this.AddType(type, typeKind, null);
        }

        /// <summary>
        /// Registers the given type to the schema. It will be made available
        /// in the type system and queriable via introspection. This method can be used to register
        /// graph types, controllers and directives.
        /// </summary>
        /// <typeparam name="TType">The type to add.</typeparam>
        /// <param name="typeKind">The kind of graph type to register the type as. Standard POCOs will
        /// default to OBJECT unless otherwise specified.</param>
        /// <returns>SchemaOptions.</returns>
        public SchemaOptions AddType<TType>(TypeKind? typeKind = null)
        {
            return this.AddType(typeof(TType), typeKind);
        }

        private SchemaOptions AddType(Type type, TypeKind? typeKind = null, ServiceLifetime? customLifeForServices = null)
        {
            Validation.ThrowIfNull(type, nameof(type));

            if (Validation.IsCastable<GraphDirective>(type))
            {
                if (typeKind != null && typeKind != TypeKind.DIRECTIVE)
                {
                    throw new SchemaConfigurationException($"The type {type.GetType().FriendlyName()} is a directive but " +
                        $"was attempted to be registered as a '{typeKind.Value}'.");
                }

                typeKind = null;
            }
            else if (Validation.IsCastable<GraphController>(type))
            {
                if (typeKind != null && typeKind != TypeKind.CONTROLLER)
                {
                    throw new SchemaConfigurationException($"The type {type.GetType().FriendlyName()} is a controller but " +
                        $"was attempted to be registered as a '{typeKind.Value}'.");
                }

                typeKind = null;
            }

            var newAdd = _possibleTypes.Add(new SchemaTypeToRegister(type, typeKind));
            if (newAdd)
            {
                if (Validation.IsCastable<GraphController>(type) || Validation.IsCastable<GraphDirective>(type))
                    this.RegisterTypeAsDependentService(type, customLifeForServices);
            }

            return this;
        }

        private void RegisterTypeAsDependentService(Type type, ServiceLifetime? lifeTimeScope = null)
        {
            lifeTimeScope = lifeTimeScope ?? GraphQLProviders.GlobalConfiguration.ControllerServiceLifeTime;
            var serviceToRegister = new ServiceToRegister(
                type,
                type,
                lifeTimeScope.Value,
                false);

            _registeredServices.Add(serviceToRegister);
        }

        /// <summary>
        /// Registers a extension for this schema option.
        /// </summary>
        /// <typeparam name="TExtensionType">The type of the t extension type.</typeparam>
        /// <param name="extension">The extension.</param>
        public void RegisterExtension<TExtensionType>(TExtensionType extension)
            where TExtensionType : class, IGraphQLServerExtension
        {
            Validation.ThrowIfNull(extension, nameof(extension));

            extension.Configure(this);
            _serverExtensions.Add(extension.GetType(), extension);
        }

        /// <summary>
        /// Adds an extension to allow processing of schema instance by an extenal object
        /// before the schema is complete. The state of the schema is not garunteed
        /// when then extension is executed. It is highly likely that the schema will undergo
        /// further processing after the extension executes.
        /// </summary>
        /// <param name="extension">The extension to apply.</param>
        public void AddConfigurationExtension(ISchemaConfigurationExtension extension)
        {
            Validation.ThrowIfNull(extension, nameof(extension));
            _configExtensions.Add(extension);
        }

        /// <summary>
        /// Begins the application of a directive of the given type to items in the target
        /// schema.
        /// </summary>
        /// <typeparam name="TDirectiveType">The type of the directive to apply.</typeparam>
        /// <returns>IDirectiveInjector.</returns>
        public DirectiveApplicator ApplyDirective<TDirectiveType>()
            where TDirectiveType : GraphDirective
        {
            return this.ApplyDirective(typeof(TDirectiveType));
        }

        /// <summary>
        /// Begins the application of a directive of the given type to items in the target
        /// schema. This type must inherit from <see cref="GraphDirective"/>.
        /// </summary>
        /// <param name="directiveType">The type of the directive to apply to schema items.</param>
        /// <returns>IDirectiveInjector.</returns>
        public DirectiveApplicator ApplyDirective(Type directiveType)
        {
            Validation.ThrowIfNull(directiveType, nameof(directiveType));
            Validation.ThrowIfNotCastable<GraphDirective>(directiveType, nameof(directiveType));

            this.AddType(directiveType, null, null);
            var applicator = new DirectiveApplicator(directiveType);
            this.AddConfigurationExtension(applicator);

            return applicator;
        }

        /// <summary>
        /// Begins the application of a directive with a given name to items in the target
        /// schema. This name is case sensitive and must match the name of the directive
        /// as it exists in the target schema.
        /// </summary>
        /// <param name="directiveName">Name of the directive.</param>
        /// <returns>IDirectiveInjector.</returns>
        public DirectiveApplicator ApplyDirective(string directiveName)
        {
            directiveName = Validation.ThrowIfNullWhiteSpaceOrReturn(directiveName, nameof(directiveName));
            var applicator = new DirectiveApplicator(directiveName);
            this.AddConfigurationExtension(applicator);

            return applicator;
        }

        /// <summary>
        /// Instructs this options collection to gather all its found services
        /// and register them to the <see cref="IServiceCollection"/> for this instance.
        /// </summary>
        internal void FinalizeServiceRegistration()
        {
            foreach (var service in _registeredServices)
            {
                var descriptor = service.CreateServiceDescriptor();
                if (service.Required)
                    this.ServiceCollection.Add(descriptor);
                else
                    this.ServiceCollection.TryAdd(descriptor);
            }
        }

        /// <summary>
        /// Gets the classes, enums, structs and other types that need to be
        /// registered to the schema when its created.
        /// </summary>
        /// <value>The registered schema types.</value>
        public IEnumerable<SchemaTypeToRegister> SchemaTypesToRegister => _possibleTypes;

        /// <summary>
        /// Gets the configuration extensions that will be applied to the schema instance when its
        /// created.
        /// </summary>
        /// <value>The configuration extensions.</value>
        public IEnumerable<ISchemaConfigurationExtension> ConfigurationExtensions => _configExtensions;

        /// <summary>
        /// Gets or sets a value indicating whether any <see cref="GraphController"/>, <see cref="GraphDirective"/>  or
        /// any classes that implement at least one graph attribute, that are part of the entry assembly, are automatically
        /// added to the <see cref="ISchema"/>. (Default: true).
        /// </summary>
        /// <value><c>true</c> if this schema should auto-register graph controllers declared
        /// on the entry assembly; otherwise, false.</value>
        public bool AutoRegisterLocalGraphEntities { get; set; } = true;

        /// <summary>
        /// Gets the options related to the runtime setup and declaration of this schema.
        /// </summary>
        /// <value>The schema declaration configuration options.</value>
        public SchemaDeclarationConfiguration DeclarationOptions { get; }

        /// <summary>
        /// Gets options related to how this schema utilizes the query cache.
        /// </summary>
        /// <value>The cache options.</value>
        public SchemaQueryPlanCacheConfiguration CacheOptions { get; }

        /// <summary>
        /// Gets the options related to how the runtime will process field authorizations for this schema.
        /// </summary>
        /// <value>The authorization options.</value>
        public SchemaAuthorizationConfiguration AuthorizationOptions { get; }

        /// <summary>
        /// Gets options related to the processing of user queries against this schema at runtime.
        /// </summary>
        /// <value>The schema execution configuration options.</value>
        public SchemaExecutionConfiguration ExecutionOptions { get; }

        /// <summary>
        /// Gets the options related to the formatting of a graphql respomnse before it is sent to the requestor.
        /// </summary>
        /// <value>The schema reponse configuration options.</value>
        public SchemaResponseConfiguration ResponseOptions { get; }

        /// <summary>
        /// Gets the options related to the configuration fo the default query controller (the rest end point)
        /// that can handle requests from end users.
        /// </summary>
        /// <value>The query handler.</value>
        public SchemaQueryHandlerConfiguration QueryHandler { get; }

        /// <summary>
        /// Gets the set of options extensions added to this schema configuration.
        /// </summary>
        /// <value>The extensions.</value>
        public IReadOnlyDictionary<Type, IGraphQLServerExtension> ServerExtensions => _serverExtensions;

        /// <summary>
        /// Gets the service collection which contains all the required entries for
        /// the schema this instance represents.
        /// </summary>
        /// <value>The service collection.</value>
        public IServiceCollection ServiceCollection { get; }

        /// <summary>
        /// <para>Gets a value indicating to what depth any added graph type
        /// will be inspected for dependent services. A deeper inspection
        /// will traverse the type system deeper but may take longer to initialize.
        /// </para>
        /// <para>
        /// Default = 3 .
        /// </para>
        /// </summary>
        /// <value>The service introspection depth.</value>
        public int ServiceIntrospectionDepth { get; }
    }
}