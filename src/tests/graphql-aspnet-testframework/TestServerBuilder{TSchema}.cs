// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
// *************************************************************// project:  graphql-aspnet
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
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
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
        private readonly List<IGraphTestFrameworkComponent> _testComponents;
        private readonly TestOptions _initialSetup;

        // colleciton of types added to this server external to the AddGraphQL call.
        private readonly HashSet<Type> _additionalTypes = new HashSet<Type>();

        private readonly List<Action<ISchemaBuilder<TSchema>>> _schemaBuilderAdditions;

        private Action<SchemaOptions> _configureOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServerBuilder{TSchema}" /> class.
        /// </summary>
        /// <param name="initialSetup">A set of flags for common preconfigured settings for the test server.</param>
        public TestServerBuilder(TestOptions initialSetup = TestOptions.None)
        {
            _testComponents = new List<IGraphTestFrameworkComponent>();
            _schemaBuilderAdditions = new List<Action<ISchemaBuilder<TSchema>>>();
            _initialSetup = initialSetup;

            this.Authorization = new TestAuthorizationBuilder();
            this.Authentication = new TestAuthenticationBuilder();
            this.UserContext = new TestUserSecurityContextBuilder(this);
            this.Logging = new TestLoggingBuilder();

            this.AddTestComponent(this.Authorization);
            this.AddTestComponent(this.UserContext);
            this.AddTestComponent(this.Logging);

            var serviceCollection = new ServiceCollection();
            this.SchemaOptions = new SchemaOptions<TSchema>(serviceCollection);
        }

        /// <summary>
        /// When a test server is built the options for the graphql configuration are first
        /// loaded with the initial delcaration shortcuts setup when this server builder was created using
        /// this method.
        /// </summary>
        /// <param name="options">The options.</param>
        private void PerformInitialConfiguration(SchemaOptions options)
        {
            options.AutoRegisterLocalGraphEntities = false;

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
        public ITestServerBuilder<TSchema> AddTestComponent(IGraphTestFrameworkComponent component)
        {
            _testComponents.Add(component);
            return this;
        }

        /// <inheritdoc />
        public ITestServerBuilder<TSchema> AddGraphType<TType>()
        {
            return this.AddGraphType(typeof(TType));
        }

        /// <inheritdoc />
        public ITestServerBuilder<TSchema> AddGraphType(Type type)
        {
            _additionalTypes.Add(type);
            return this;
        }

        /// <inheritdoc />
        public ITestServerBuilder<TSchema> AddGraphController<TController>()
            where TController : GraphController
        {
            return this.AddGraphType<TController>();
        }

        /// <inheritdoc />
        public ITestServerBuilder<TSchema> AddGraphQL(Action<SchemaOptions> configureOptions)
        {
            _configureOptions = configureOptions;
            return this;
        }

        /// <inheritdoc />
        public ITestServerBuilder<TSchema> AddSchemaBuilderAction(Action<ISchemaBuilder<TSchema>> action)
        {
            _schemaBuilderAdditions.Add(action);
            return this;
        }

        /// <inheritdoc />
        public TestServer<TSchema> Build()
        {
            // register all test components
            foreach (var component in _testComponents)
                component.Inject(this.SchemaOptions.ServiceCollection);

            var userProvidedConfigOptions = _configureOptions;
            _configureOptions = (options) =>
            {
                this.PerformInitialConfiguration(options);
                foreach (var type in _additionalTypes)
                    options.AddGraphType(type);

                userProvidedConfigOptions?.Invoke(options);
            };

            // inject staged graph types
            var injector = new GraphQLSchemaInjector<TSchema>(this.SchemaOptions, _configureOptions);
            injector.ConfigureServices();

            foreach (var action in _schemaBuilderAdditions)
                action.Invoke(injector.SchemaBuilder);

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
    }
}