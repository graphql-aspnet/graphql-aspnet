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
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Middleware.ApolloSubscriptionQueryExecution;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A schema extension encapsulating a subscription server that can accept clients and respond to
    /// subscription events for connected clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this extension is built for.</typeparam>
    public class ApolloSubscriptionServerSchemaExtension<TSchema> : ISchemaExtension
        where TSchema : class, ISchema
    {
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

            // swap out the master providers for the ones that includes
            // support for the subscription action type
            if (!(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider))
                GraphQLProviders.TemplateProvider = new SubscriptionEnabledTemplateProvider();

            if (!(GraphQLProviders.GraphTypeMakerProvider is SubscriptionEnabledGraphTypeMakerProvider))
                GraphQLProviders.GraphTypeMakerProvider = new SubscriptionEnabledGraphTypeMakerProvider();

            // wipe out the current execution pipeline and rebuild with subscription creation middleware injected
            _schemaBuilder.QueryExecutionPipeline.Clear();
            var subscriptionQueryExecutionHelper = new SubscriptionExecutionPipelineHelper<TSchema>(_schemaBuilder.QueryExecutionPipeline);
            subscriptionQueryExecutionHelper.AddDefaultMiddlewareComponents();

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
                    typeof(ApolloSubscriptionServer<TSchema>),
                    ServiceLifetime.Singleton));

            if (this.SubscriptionOptions.HttpMiddlewareComponentType != null)
                this.EnsureMiddlewareTypeOrThrow(this.SubscriptionOptions.HttpMiddlewareComponentType);
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
                var routePath = this.SubscriptionOptions.Route.Replace(
                SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY,
                _primaryOptions.QueryHandler.Route);

                var server = serviceProvider.GetRequiredService<ISubscriptionServer<TSchema>>();

                var middlewareType = this.SubscriptionOptions.HttpMiddlewareComponentType
                    ?? typeof(DefaultGraphQLHttpSubscriptionMiddleware<TSchema>);

                this.EnsureMiddlewareTypeOrThrow(middlewareType);

                // register the middleware component
                app.UseMiddleware(middlewareType, server, this.SubscriptionOptions, routePath);
                app.ApplicationServices.WriteLogEntry(
                      (l) => l.SchemaSubscriptionRouteRegistered<TSchema>(
                      routePath));
            }
        }

        /// <summary>
        /// Ensures the middleware type contains a public constructor that accepts the
        /// three parameters required of it by the runtime.
        /// </summary>
        /// <param name="middlewareType">Type of the middleware to inspect.</param>
        private void EnsureMiddlewareTypeOrThrow(Type middlewareType)
        {
            var constructor = middlewareType.GetConstructor(
                new[]
                {
                    typeof(RequestDelegate),
                    typeof(ISubscriptionServer<TSchema>),
                    typeof(SubscriptionServerOptions<TSchema>),
                    typeof(string),
                });

            if (constructor == null)
            {
                throw new InvalidOperationException(
                      $"Unable to initialize subscriptions for schema '{typeof(TSchema).FriendlyName()}'. " +
                      $"An attempt was made to use the type '{middlewareType.FriendlyName()}' as the middleware " +
                      "component to handle subscription operation requests. However, this type does not contain a public " +
                      $"constructor that accepts parameters of {typeof(RequestDelegate).FriendlyName()}, {typeof(SubscriptionServerOptions<TSchema>)}, and {typeof(string)}.");
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