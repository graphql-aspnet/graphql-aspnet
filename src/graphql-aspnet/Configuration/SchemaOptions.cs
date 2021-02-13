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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A complete set of configuration options to setup a schema.
    /// </summary>
    public class SchemaOptions
    {
        private readonly Dictionary<Type, ISchemaExtension> _extensions;
        private readonly HashSet<Type> _possibleTypes;
        private readonly Type _schemaType;

        /// <summary>
        /// Occurs when a type reference is set to this configuration section that requires injection into the service collection.
        /// </summary>
        internal event EventHandler<TypeReferenceEventArgs> TypeReferenceAdded;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaOptions" /> class.
        /// </summary>
        /// <param name="schemaType">Type of the schema being built.</param>
        public SchemaOptions(Type schemaType)
        {
            _schemaType = Validation.ThrowIfNullOrReturn(schemaType, nameof(schemaType));
            _possibleTypes = new HashSet<Type>();
            this.DeclarationOptions = new SchemaDeclarationConfiguration();
            this.CacheOptions = new SchemaQueryPlanCacheConfiguration();
            this.AuthorizationOptions = new SchemaAuthorizationConfiguration();
            this.ExecutionOptions = new SchemaExecutionConfiguration();
            this.ResponseOptions = new SchemaResponseConfiguration();
            this.QueryHandler = new SchemaQueryHandlerConfiguration();
            _extensions = new Dictionary<Type, ISchemaExtension>();
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
        /// found on the target <see cref="ISchema" /> assembly, injecting them into the schema and generating the
        /// appropriate graph types.
        /// </summary>
        /// <returns>SchemaConfiguration&lt;TSchema&gt;.</returns>
        public SchemaOptions AddSchemaAssembly()
        {
            var assemblyToCheck = _schemaType.Assembly;
            return this.AddGraphAssembly(assemblyToCheck);
        }

        /// <summary>
        /// Searches for and registers all publically accessable <see cref="GraphController" /> and <see cref="GraphDirective"/>
        /// found on the supplied assembly, injecting them into the schema as graph types.
        /// </summary>
        /// <param name="assembly">The assembly to scan for items.</param>
        /// <returns>SchemaConfiguration&lt;TSchema&gt;.</returns>
        public SchemaOptions AddGraphAssembly(Assembly assembly)
        {
            Validation.ThrowIfNull(assembly, nameof(assembly));
            var typesToAdd = assembly.LocateTypesInAssembly(Constants.AssemblyScanTypes);
            foreach (var type in typesToAdd)
                this.AddGraphType(type);

            return this;
        }

        /// <summary>
        /// Registers the type to the schema when it is instaniated, it will be made available
        /// in the type system and queriable via introspection.
        /// </summary>
        /// <typeparam name="TItem">The type to add.</typeparam>
        /// <returns>SchemaConfiguration&lt;TSchema&gt;.</returns>
        public SchemaOptions AddGraphType<TItem>()
        {
            return this.AddGraphType(typeof(TItem));
        }

        /// <summary>
        /// Registers the type to the schema when it is instaniated, it will be made available
        /// in the type system and queriable via introspection.
        /// </summary>
        /// <param name="type">The type to add.</param>
        /// <returns>SchemaConfiguration&lt;TSchema&gt;.</returns>
        public SchemaOptions AddGraphType(Type type)
        {
            Validation.ThrowIfNull(type, nameof(type));
            var newAdd = _possibleTypes.Add(type);
            if (newAdd)
            {
                if (Validation.IsCastable<GraphController>(type) || Validation.IsCastable<GraphDirective>(type))
                    this.TypeReferenceAdded?.Invoke(this, new TypeReferenceEventArgs(type, ServiceLifetime.Scoped));
            }

            return this;
        }

        /// <summary>
        /// Registers a extension for this schema option.
        /// </summary>
        /// <typeparam name="TExtensionType">The type of the t extension type.</typeparam>
        /// <param name="extension">The extension.</param>
        public void RegisterExtension<TExtensionType>(TExtensionType extension)
            where TExtensionType : class, ISchemaExtension
        {
            Validation.ThrowIfNull(extension, nameof(extension));

            extension.Configure(this);
            _extensions.Add(extension.GetType(), extension);

            if (extension.RequiredServices != null)
            {
                foreach (var descriptor in extension.RequiredServices)
                {
                    this.TypeReferenceAdded?.Invoke(this, new TypeReferenceEventArgs(descriptor, true));
                }
            }

            if (extension.OptionalServices != null)
            {
                foreach (var descriptor in extension.OptionalServices)
                {
                    this.TypeReferenceAdded?.Invoke(this, new TypeReferenceEventArgs(descriptor, false));
                }
            }
        }

        /// <summary>
        /// Gets the registered schema types.
        /// </summary>
        /// <value>The registered schema types.</value>
        public IEnumerable<Type> RegisteredSchemaTypes => _possibleTypes;

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
        public IReadOnlyDictionary<Type, ISchemaExtension> Extensions => _extensions;
    }
}