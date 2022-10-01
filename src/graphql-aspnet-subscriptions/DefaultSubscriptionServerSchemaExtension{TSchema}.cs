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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Exceptions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.SubcriptionExecution;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// A schema extension encapsulating a subscription server that can accept clients and respond to
    /// subscription events for connected clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this extension is built for.</typeparam>
    public class DefaultSubscriptionServerSchemaExtension<TSchema> : IGraphQLServerExtension
        where TSchema : class, ISchema
    {
        /// <summary>
        /// The required authentication method necessary for this extension to function.
        /// </summary>
        public const AuthorizationMethod REQUIRED_AUTH_METHOD = AuthorizationMethod.PerRequest;

        private readonly ISchemaBuilder<TSchema> _schemaBuilder;
        private SchemaOptions _primaryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionServerSchemaExtension{TSchema}" /> class.
        /// </summary>
        /// <param name="schemaBuilder">The schema builder created when adding graphql to the server
        /// originally.</param>
        /// <param name="options">The options.</param>
        public DefaultSubscriptionServerSchemaExtension(ISchemaBuilder<TSchema> schemaBuilder, SubscriptionServerOptions<TSchema> options)
        {
            _schemaBuilder = Validation.ThrowIfNullOrReturn(schemaBuilder, nameof(schemaBuilder));
            this.SubscriptionOptions = Validation.ThrowIfNullOrReturn(options, nameof(options));
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
            _primaryOptions = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _primaryOptions.DeclarationOptions.AllowedOperations.Add(GraphCollection.Subscription);

            // enforce a "per request" auth method for the server instance
            // if and only if an auth method was not set explicitly by the developer
            if (!_primaryOptions.AuthorizationOptions.Method.HasValue)
                _primaryOptions.AuthorizationOptions.Method = REQUIRED_AUTH_METHOD;

            // Security requirement for this component
            // --------------------------
            // this component MUST use per request authorization.
            // a subscription query is checked BEFORE its registered
            // as opposed to when its executed
            var authMethod = _primaryOptions.AuthorizationOptions.Method;
            if (!authMethod.HasValue || authMethod != REQUIRED_AUTH_METHOD)
            {
                throw new SubscriptionServerException(
                    $"Invalid Authorization Method. The default subscription server requires a \"{REQUIRED_AUTH_METHOD}\" " +
                    $"authorization method. (Current authorization method is \"{_schemaBuilder.Options.AuthorizationOptions.Method}\")");
            }

            // swap out the master templating provider for the one that includes
            // support for the subscription action type if and only if the developer has not
            // already registered their own custom one
            if (GraphQLProviders.TemplateProvider == null || GraphQLProviders.TemplateProvider.GetType() == typeof(DefaultTypeTemplateProvider))
                GraphQLProviders.TemplateProvider = new SubscriptionEnabledTemplateProvider();

            // swap out the master graph type maker to its "subscription enabled" version
            // if and only if the developer has not already registered their own custom instance
            if (GraphQLProviders.GraphTypeMakerProvider == null || GraphQLProviders.GraphTypeMakerProvider.GetType() == typeof(DefaultGraphTypeMakerProvider))
                GraphQLProviders.GraphTypeMakerProvider = new SubscriptionEnabledGraphTypeMakerProvider();

            // Update the query execution pipeline
            // ------------------------------------------
            // Wipe out the current execution pipeline and rebuild with
            // subscription middleware injected
            _schemaBuilder.QueryExecutionPipeline.Clear();
            var subscriptionQueryExecutionHelper = new SubscriptionQueryExecutionPipelineHelper<TSchema>(_schemaBuilder.QueryExecutionPipeline);
            subscriptionQueryExecutionHelper.AddDefaultMiddlewareComponents(_primaryOptions);

            // Update field execution pipeline
            // -----------------------------
            // because the authorization method may have changed
            // rebuild the field execution pipeline as well.
            // This may happen when no method is declared for 'options.AuthorizationOptions.Method'
            // allowing the defaults to propegate during pipeline creation.
            // The default for the basic server is "per field"
            // The required value for subscriptions is "per request"
            _schemaBuilder.FieldExecutionPipeline.Clear();
            var fieldExecutionHelper = new FieldExecutionPipelineHelper<TSchema>(_schemaBuilder.FieldExecutionPipeline);
            fieldExecutionHelper.AddDefaultMiddlewareComponents(_primaryOptions);

            // register the primary subscription options for the schema
            // so they are available to anyone that needs them
            _schemaBuilder.Options.ServiceCollection.Add(
                new ServiceDescriptor(
                    typeof(SubscriptionServerOptions<TSchema>),
                    this.SubscriptionOptions));

            // register dependencies for built-in websocket sub protocols
            _schemaBuilder.Options.ServiceCollection.AddGraphqlWsLegacysProtocol();
            _schemaBuilder.Options.ServiceCollection.AddGqltwsProtocol();

            _schemaBuilder.Options.ServiceCollection.TryAdd(
             new ServiceDescriptor(
                 typeof(ISubscriptionServerClientFactory),
                 typeof(DefaultSubscriptionServerClientFactory),
                 ServiceLifetime.Singleton));
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
    }
}