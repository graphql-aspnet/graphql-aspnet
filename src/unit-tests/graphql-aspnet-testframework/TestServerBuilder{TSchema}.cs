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
        private readonly List<(Type Type, TypeKind? TypeKind)> _additionalTypes = new List<(Type, TypeKind?)>();

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
            TestAuthenticationBuilder authenticationBuilder = null,
            TestAuthorizationBuilder authorizationBuilder = null,
            TestUserSecurityContextBuilder securityContextBuilder = null,
            TestLoggingBuilder loggingBuilder = null)
        {
            _schemaBuilderAdditions = new List<Action<ISchemaBuilder<TSchema>>>();
            _initialSetup = initialSetup;

            this.Authentication = authenticationBuilder ?? new TestAuthenticationBuilder();
            this.Authorization = authorizationBuilder ?? new TestAuthorizationBuilder();
            this.UserContext = securityContextBuilder ?? new TestUserSecurityContextBuilder(this);
            this.Logging = loggingBuilder ?? new TestLoggingBuilder();

            this.TestComponents = new List<IGraphQLTestFrameworkComponent>();
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
                options.DeclarationOptions.GraphNamingFormatter = new GraphNameFormatter(GraphNameFormatStrategy.NoChanges);
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
        public virtual ITestServerBuilder<TSchema> AddType<TType>(TypeKind? typeKind = null)
        {
            return this.AddType(typeof(TType), typeKind);
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddType(Type type, TypeKind? typeKind = null)
        {
            _additionalTypes.Add((type, typeKind));
            return this;
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddGraphController<TController>()
            where TController : GraphController
        {
            this.AddType(typeof(TController));
            return this;
        }

        /// <inheritdoc />
        public virtual ITestServerBuilder<TSchema> AddDirective<TDirective>()
            where TDirective : GraphDirective
        {
            this.AddType(typeof(TDirective));
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
                foreach (var addType in _additionalTypes)
                    options.AddType(addType.Type, addType.TypeKind);

                _startupConfigureOptionsAction?.Invoke(options);
            };

            // perform a schema injection to setup all the registered
            // graph types for the schema
            var injector = GraphQLSchemaInjectorFactory.Create(this.SchemaOptions, masterConfigMethod);
            injector.ConfigureServices();

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
        public SchemaOptions<TSchema> SchemaOptions { get; }

        /// <inheritdoc />
        public TestAuthenticationBuilder Authentication { get; }

        /// <inheritdoc />
        public TestAuthorizationBuilder Authorization { get; }

        /// <inheritdoc />
        public TestUserSecurityContextBuilder UserContext { get; }

        /// <inheritdoc />
        public TestLoggingBuilder Logging { get; }

        /// <summary>
        /// Gets a list of test components currently registered to this instance.
        /// </summary>
        /// <value>The registered test components.</value>
        protected IList<IGraphQLTestFrameworkComponent> TestComponents { get; }
    }
}