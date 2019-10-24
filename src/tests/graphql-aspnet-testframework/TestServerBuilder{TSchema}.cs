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
    using System.Security.Claims;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using GraphQL.AspNet.Tests.Framework.ServerBuilders;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder class to configure a scenario and generate a test server to execute unit tests against.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public partial class TestServerBuilder<TSchema> : ServiceCollection
        where TSchema : class, ISchema, new()
    {
        private readonly List<IGraphTestFrameworkComponent> _testComponents;
        private readonly TestOptions _initialSetup;

        // colleciton of types added to this server external to the AddGraphQL call.
        private readonly HashSet<Type> _additionalTypes = new HashSet<Type>();

        private Action<SchemaOptions> _configureOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServerBuilder{TSchema}" /> class.
        /// </summary>
        /// <param name="initialSetup">A set of flags for common preconfigured settings for the test server.</param>
        public TestServerBuilder(TestOptions initialSetup = TestOptions.None)
        {
            _testComponents = new List<IGraphTestFrameworkComponent>();
            _initialSetup = initialSetup;
            this.Authorization = new TestAuthorizationBuilder();
            this.User = new TestUserAccountBuilder();
            this.Logging = new TestLoggingBuilder();

            this.AddTestComponent(this.Authorization);
            this.AddTestComponent(this.User);
            this.AddTestComponent(this.Logging);
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

            if (_initialSetup.HasFlag(TestOptions.CodeDeclaredNames))
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

        /// <summary>
        /// Adds the test component to those injected into the service provider when the server is built.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        public TestServerBuilder<TSchema> AddTestComponent(IGraphTestFrameworkComponent component)
        {
            _testComponents.Add(component);
            return this;
        }

        /// <summary>
        /// Helpful overload for direct access to inject a graph type into the server.
        /// </summary>
        /// <typeparam name="TType">The concrete type to parse when creating a graph type.</typeparam>
        /// <returns>TestServerBuilder.</returns>
        public TestServerBuilder<TSchema> AddGraphType<TType>()
        {
            return this.AddGraphType(typeof(TType));
        }

        /// <summary>
        /// Helpful overload for direct access to inject a graph type into the server.
        /// </summary>
        /// <param name="type">The type to inject.</param>
        /// <returns>TestServerBuilder.</returns>
        public TestServerBuilder<TSchema> AddGraphType(Type type)
        {
            _additionalTypes.Add(type);
            return this;
        }

        /// <summary>
        /// Adds the graph controller to the server and renders it and its dependents into the target schema.
        /// </summary>
        /// <typeparam name="TController">The type of the t controller.</typeparam>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        public TestServerBuilder<TSchema> AddGraphController<TController>()
            where TController : GraphController
        {
            return this.AddGraphType<TController>();
        }

        /// <summary>
        /// Provides access to the options configuration function used when graphQL is first added
        /// to a server instance.
        /// </summary>
        /// <param name="configureOptions">The function to invoke to configure graphql settings.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        public TestServerBuilder<TSchema> AddGraphQL(Action<SchemaOptions> configureOptions)
        {
            _configureOptions = configureOptions;
            return this;
        }

        /// <summary>
        /// Creates a new test server instance from the current settings in this builder.
        /// </summary>
        /// <returns>TestServer.</returns>
        public TestServer<TSchema> Build()
        {
            var serviceCollection = new ServiceCollection();

            // any additional, 1 off services added to the builder?
            foreach (var service in this)
                serviceCollection.AddOrUpdate(service);

            // register all test components
            foreach (var component in _testComponents)
                component.Inject(serviceCollection);

            var userProvidedConfigOptions = _configureOptions;
            _configureOptions = (options) =>
            {
                this.PerformInitialConfiguration(options);
                foreach (var type in _additionalTypes)
                    options.AddGraphType(type);

                userProvidedConfigOptions?.Invoke(options);
            };

            // inject staged graph types
            var injector = new GraphQLSchemaInjector<TSchema>(serviceCollection, _configureOptions);
            injector.ConfigureServices();

            var userAccount = this.User.CreateUserAccount();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            injector.UseSchema(serviceProvider);

            return new TestServer<TSchema>(serviceProvider, userAccount);
        }

        /// <summary>
        /// Gets the authorization builder used to configure the roles and policys known the test server.
        /// </summary>
        /// <value>The authorization.</value>
        public TestAuthorizationBuilder Authorization { get; }

        /// <summary>
        /// Gets the builder to configure the creation of a mocked <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <value>The user.</value>
        public TestUserAccountBuilder User { get; }

        /// <summary>
        /// Gets the builder to configure the setup of the logging framework.
        /// </summary>
        /// <value>The logging.</value>
        public TestLoggingBuilder Logging { get; }
    }
}