// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Configuration.Startup;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using GraphQL.AspNet.Tests.Framework.ServerBuilders;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder class to configure a scenario and generate a test server to execute unit tests against.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema being built by this instance.</typeparam>
    public partial class TestServerBuilder<TSchema> : ITestServerBuilder<TSchema>
        where TSchema : class, ISchema
    {
        private readonly TestOptions _initialSetup;

        // colleciton of types added to this server external to the AddGraphQL call.
        private readonly List<(Type Type, TypeKind? TypeKind, ServiceLifetime? Lifetime)> _additionalTypes = new List<(Type, TypeKind?, ServiceLifetime?)>();

        private readonly List<Action<ISchemaBuilder<TSchema>>> _schemaBuilderAdditions;

        private Action<SchemaOptions> _startupConfigureOptionsAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServerBuilder{TSchema}" /> class.
        /// </summary>
        /// <param name="initialSetup">
        /// A set of bitwise flags to denote for commonly used, preconfigured
        /// settings for the test server.
        /// </param>
        /// <param name="serviceCollection">
        /// A customized service collection to use in this server. When <c>null</c>,
        /// a new service collection instance will be automatically created.
        /// </param>
        /// <param name="authenticationBuilder">
        /// A customized authentication builder. When <c>null</c>,
        /// a new, default instance will be automatically created.
        /// </param>
        /// <param name="authorizationBuilder">
        /// A customized authorization builder.  When <c>null</c>,
        /// a new, default instance will be automatically created.
        /// </param>
        /// <param name="securityContextBuilder">
        /// A customized user security context builder.  When <c>null</c>,
        /// a new, default instance will be automatically created.
        /// </param>
        /// <param name="loggingBuilder">
        /// A customed logging builder. When <c>null</c>,
        /// a new, default instance will be automatically created.
        /// </param>
        public TestServerBuilder(
            TestOptions initialSetup = TestOptions.None,
            IServiceCollection serviceCollection = null,
            ITestAuthenticationBuilder authenticationBuilder = null,
            ITestAuthorizationBuilder authorizationBuilder = null,
            ITestUserSecurityContextBuilder securityContextBuilder = null,
            ITestLoggingBuilder loggingBuilder = null)
        {
            _schemaBuilderAdditions = new List<Action<ISchemaBuilder<TSchema>>>();
            _initialSetup = initialSetup;

            this.Authentication = authenticationBuilder ?? new TestAuthenticationBuilder<TSchema>();
            this.Authorization = authorizationBuilder ?? new TestAuthorizationBuilder<TSchema>();
            this.UserContext = securityContextBuilder ?? new TestUserSecurityContextBuilder<TSchema>(this);
            this.Logging = loggingBuilder ?? new TestLoggingBuilder<TSchema>();

            this.TestComponents = new List<IGraphQLTestFrameworkComponent>();
            this.AddTestComponent(this.Authentication);
            this.AddTestComponent(this.Authorization);
            this.AddTestComponent(this.UserContext);
            this.AddTestComponent(this.Logging);

            serviceCollection = serviceCollection ?? new ServiceCollection();
            this.SchemaOptions = new SchemaOptions<TSchema>(serviceCollection);
        }

        /// <summary>
        /// When a test server is built the options for the graphql configuration are first
        /// loaded with the initial delcaration shortcuts setup when this server builder was created using
        /// this method.
        /// </summary>
        /// <param name="options">The options.</param>
        protected virtual void PerformInitialConfiguration(SchemaOptions options)
        {
            options.AutoRegisterLocalEntities = false;

            if (_initialSetup.HasFlag(TestOptions.UseCodeDeclaredNames))
            {
                options.DeclarationOptions.SchemaFormatStrategy = new SchemaFormatStrategy(SchemaItemNameFormatOptions.NoChanges);
            }

            if (_initialSetup.HasFlag(TestOptions.IncludeExceptions))
            {
                options.ResponseOptions.ExposeExceptions = true;
            }

            if (_initialSetup.HasFlag(TestOptions.IncludeMetrics))
            {
                options.ExecutionOptions.EnableMetrics = true;
                options.ResponseOptions.ExposeMetrics = true;
            }
        }

        /// <inheritdoc />
        public ITestServerBuilder<TSchema> AddTestComponent(IGraphQLTestFrameworkComponent component)
        {
            this.TestComponents.Add(component);
            return this;
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddGraphType<TType>(TypeKind? typeKind = null)
        {
            return this.AddType(typeof(TType), typeKind, null);
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddGraphType(Type type, TypeKind? typeKind = null)
        {
            return this.AddType(type, typeKind, null);
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddType<TType>(TypeKind? typeKind = null)
        {
            return this.AddType(typeof(TType), typeKind, null);
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddType(Type type, TypeKind? typeKind = null)
        {
            return this.AddType(type, typeKind, null);
        }

        /// <summary>
        /// Adds the complete type definition that will be parsed and put into the schema.
        /// </summary>
        /// <param name="type">The .NET object type being added.</param>
        /// <param name="typeKind">A specific GraphQL typekind, if needed (ignored for types determined to be controllers or directives).</param>
        /// <param name="customLifetime">A custom registration lifetime (ignored for types deteremined to be a standard graph type).</param>
        /// <returns>ITestServerBuilder&lt;TSchema&gt;.</returns>
        protected ITestServerBuilder<TSchema> AddType(Type type, TypeKind? typeKind = null, ServiceLifetime? customLifetime = null)
        {
            _additionalTypes.Add((type, typeKind, customLifetime));
            return this;
        }

        /// <inheritdoc cref="ITestServerBuilder{TSchema}.AddController{TController}(ServiceLifetime?)" />
        public virtual ITestServerBuilder<TSchema> AddGraphController<TController>(ServiceLifetime? customLifetime = null)
            where TController : GraphController
        {
            return this.AddController<TController>(customLifetime);
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddController<TController>(ServiceLifetime? customLifetime = null)
            where TController : GraphController
        {
            this.AddType(typeof(TController), null, customLifetime);
            return this;
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddDirective<TDirective>(ServiceLifetime? customLifetime = null)
            where TDirective : GraphDirective
        {
            this.AddType(typeof(TDirective), null, customLifetime);
            return this;
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddGraphQL(Action<SchemaOptions> configureOptions)
        {
            _startupConfigureOptionsAction = configureOptions;
            return this;
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddSchemaBuilderAction(Action<ISchemaBuilder<TSchema>> action)
        {
            _schemaBuilderAdditions.Add(action);
            return this;
        }

        /// <inheritdoc />
        public virtual TestServer<TSchema> Build()
        {
            // register all test components
            foreach (var component in this.TestComponents)
                component.Inject(this.SchemaOptions.ServiceCollection);

            // create a master configuration method that performs the initial
            // setup requested during construction
            // along with a user provided configuration if provided
            Action<SchemaOptions> masterConfigMethod = (options) =>
            {
                this.PerformInitialConfiguration(options);

                // configure all test components
                foreach (var component in this.TestComponents)
                    component.Configure(options);

                foreach (var addType in _additionalTypes)
                    options.AddType(addType.Type, addType.TypeKind);

                _startupConfigureOptionsAction?.Invoke(options);
            };

            // perform a schema injection to setup all the registered
            // graph types for the schema in the DI container
            var wasFound = GraphQLSchemaInjectorFactory.TryGetOrCreate(
                out var injector,
                this.SchemaOptions,
                masterConfigMethod);

            if (!wasFound)
            {
                injector.ConfigureServices();
            }

            // allow the typed test components to do their thing with the
            // schema builder
            foreach (var component in this.TestComponents.Where(x => x is IGraphQLTestFrameworkComponent<TSchema>))
                ((IGraphQLTestFrameworkComponent<TSchema>)component).Configure(injector.SchemaBuilder);

            // execute any additional explicit actions for the generated schema builder
            foreach (var action in _schemaBuilderAdditions)
                action.Invoke(injector.SchemaBuilder);

            // finalize the test user instance and the service provider used at runtime
            var userSecurityContext = this.UserContext.CreateSecurityContext();
            var serviceProvider = this.SchemaOptions.ServiceCollection.BuildServiceProvider();

            injector.UseSchema(serviceProvider);
            return new TestServer<TSchema>(serviceProvider, userSecurityContext);
        }

        /// <inheritdoc />
        public SchemaOptions<TSchema> SchemaOptions { get; protected set; }

        /// <inheritdoc />
        public ITestAuthenticationBuilder Authentication { get; protected set; }

        /// <inheritdoc />
        public ITestAuthorizationBuilder Authorization { get; protected set; }

        /// <inheritdoc />
        public ITestUserSecurityContextBuilder UserContext { get; protected set; }

        /// <inheritdoc />
        public ITestLoggingBuilder Logging { get; protected set; }

        /// <summary>
        /// Gets a list of test components currently registered to this builder instance. These
        /// components will be used to construct a server instance.
        /// </summary>
        /// <value>The registered test components.</value>
        protected IList<IGraphQLTestFrameworkComponent> TestComponents { get; }
    }
}