// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Apollo.Exceptions;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.SubcriptionExecution;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A schema extension encapsulating a subscription server that can accept clients and respond to
    /// subscription events for connected clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this extension is built for.</typeparam>
    public class ApolloSubscriptionServerSchemaExtension<TSchema> : ISchemaExtension
        where TSchema : class, ISchema
    {
        /// <summary>
        /// The required authentication method necessary for this extension to function.
        /// </summary>
        public const AuthorizationMethod REQUIRED_AUTH_METHOD = AuthorizationMethod.PerRequest;

        private readonly ISchemaBuilder<TSchema> _schemaBuilder;
        private SchemaOptions _primaryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionServerSchemaExtension{TSchema}" /> class.
        /// </summary>
        /// <param name="schemaBuilder">The schema builder created when adding graphql to the server
        /// originally.</param>
        /// <param name="options">The options.</param>
        public ApolloSubscriptionServerSchemaExtension(ISchemaBuilder<TSchema> schemaBuilder, SubscriptionServerOptions<TSchema> options)
        {
            _schemaBuilder = Validation.ThrowIfNullOrReturn(schemaBuilder, nameof(schemaBuilder));
            this.SubscriptionOptions = Validation.ThrowIfNullOrReturn(options, nameof(options));
            this.RequiredServices = new List<ServiceDescriptor>();
            this.OptionalServices = new List<ServiceDescriptor>();
        }

        /// <summary>
        /// This method is called by the parent options just before it is added to the extensions
        /// collection. Use this method to do any sort of configuration, final default settings etc.
        /// This method represents the last opportunity for the extention options to modify its own required
        /// service collection before being incorporated with the DI container.
        /// </summary>
        /// <param name="options">The parent options which owns this extension.</param>
        public virtual void Configure(SchemaOptions options)
        {
            _primaryOptions = options;
            _primaryOptions.DeclarationOptions.AllowedOperations.Add(GraphCollection.Subscription);

            // enforce a default auth method for the server instance
            // if one was not set explicitly by the developer
            if (!_primaryOptions.AuthorizationOptions.Method.HasValue)
                _primaryOptions.AuthorizationOptions.Method = REQUIRED_AUTH_METHOD;

            // Security requirement for this component
            // --------------------------
            // this component MUST use per request authorization
            // a subscription query is then checked before its registered
            // as opposed to when its executed
            var authMethod = _primaryOptions.AuthorizationOptions.Method;
            if (!authMethod.HasValue || authMethod != REQUIRED_AUTH_METHOD)
            {
                throw new ApolloSubscriptionServerException(
                    $"Invalid Authorization Method. The default, apollo compliant, subscription server requires a \"{REQUIRED_AUTH_METHOD}\" " +
                    $"authorization method. (Current authorization method is \"{_schemaBuilder.Options.AuthorizationOptions.Method}\")");
            }

            // swap out the master providers for the ones that includes
            // support for the subscription action type
            if (!(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider))
                GraphQLProviders.TemplateProvider = new SubscriptionEnabledTemplateProvider();

            if (!(GraphQLProviders.GraphTypeMakerProvider is SubscriptionEnabledGraphTypeMakerProvider))
                GraphQLProviders.GraphTypeMakerProvider = new SubscriptionEnabledGraphTypeMakerProvider();

            // Update the query execution pipeline
            // ------------------------------------------
            // wipe out the current execution pipeline and rebuild with subscription creation middleware injected
            _schemaBuilder.QueryExecutionPipeline.Clear();
            var subscriptionQueryExecutionHelper = new SubscriptionQueryExecutionPipelineHelper<TSchema>(_schemaBuilder.QueryExecutionPipeline);
            subscriptionQueryExecutionHelper.AddDefaultMiddlewareComponents(_primaryOptions);

            // Update field execution pipeline
            // -----------------------------
            // because the authorization method may have changed
            // rebuild the field execution pipeline as well.
            // This may happen when no method is declared for 'options.AuthorizationOptions.Method'
            // allowing the defaults to propegate during pipeline creation.            //
            // The default for the basic server is "per field"
            // The required value for subscriptions is "per request"
            _schemaBuilder.FieldExecutionPipeline.Clear();
            var fieldExecutionHelper = new FieldExecutionPipelineHelper<TSchema>(_schemaBuilder.FieldExecutionPipeline);
            fieldExecutionHelper.AddDefaultMiddlewareComponents(_primaryOptions);

            // the primary subscription options for the schema
            this.RequiredServices.Add(new ServiceDescriptor(typeof(SubscriptionServerOptions<TSchema>), this.SubscriptionOptions));

            this.RequiredServices.Add(
                new ServiceDescriptor(
                    typeof(ApolloMessageConverterFactory),
                    typeof(ApolloMessageConverterFactory),
                    ServiceLifetime.Singleton));

            // add the needed apollo's classes as optional services
            // if the user has already added support for their own handlers
            // they will be safely ignored
            this.OptionalServices.Add(
                new ServiceDescriptor(
                    typeof(ISubscriptionServer<TSchema>),
                    this.CreateSubscriptionServer,
                    ServiceLifetime.Singleton));
        }

        /// <summary>
        /// Creates the apollo subscription server and logs its creation.
        /// </summary>
        /// <param name="sp">The service provider to create the server from.</param>
        /// <returns>ISubscriptionServer&lt;TSchema&gt;.</returns>
        private ISubscriptionServer<TSchema> CreateSubscriptionServer(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var schema = scope.ServiceProvider.GetRequiredService<TSchema>();
            var eventListener = scope.ServiceProvider.GetRequiredService<ISubscriptionEventRouter>();
            var logger = scope.ServiceProvider.GetService<IGraphEventLogger>();

            var server = new ApolloSubscriptionServer<TSchema>(schema, this.SubscriptionOptions, eventListener, logger);
            logger?.SubscriptionServerCreated(server);
            return server;
        }

        /// <summary>
        /// Invokes this instance to perform any final setup requirements as part of
        /// its configuration during startup.
        /// </summary>
        /// <param name="app">The application builder, no middleware will be registered if not supplied.</param>
        /// <param name="serviceProvider">The service provider to use.</param>
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
            serviceProvider ??= app?.ApplicationServices;

            // configure the subscription route middleware for invoking the graphql
            // pipeline for the subscription
            if (!this.SubscriptionOptions.DisableDefaultRoute && app != null)
            {
                var middlewareType = this.SubscriptionOptions.HttpMiddlewareComponentType
                    ?? typeof(DefaultGraphQLHttpSubscriptionMiddleware<TSchema>);

                this.SubscriptionOptions.Route = this.SubscriptionOptions.Route.Replace(
                    SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY,
                    _primaryOptions.QueryHandler.Route);

                // register the middleware component
                app.UseMiddleware(middlewareType);

                var logger = serviceProvider.CreateScope().ServiceProvider.GetService<IGraphEventLogger>();
                logger?.SchemaSubscriptionRouteRegistered<TSchema>(this.SubscriptionOptions.Route);
            }
        }

        /// <summary>
        /// Gets the options related to this extension instance.
        /// </summary>
        /// <value>The options.</value>
        public SubscriptionServerOptions<TSchema> SubscriptionOptions { get; }

        /// <summary>
        /// Gets a collection of services this extension has registered that should be included in
        /// a DI container.
        /// </summary>
        /// <value>The additional types as formal descriptors.</value>
        public List<ServiceDescriptor> RequiredServices { get; }

        /// <summary>
        /// Gets a collection of services this extension has registered that may be included in
        /// a DI container. If they cannot be added, because a reference already exists, they will be skipped.
        /// </summary>
        /// <value>The additional types as formal descriptors.</value>
        public List<ServiceDescriptor> OptionalServices { get; }
    }
}