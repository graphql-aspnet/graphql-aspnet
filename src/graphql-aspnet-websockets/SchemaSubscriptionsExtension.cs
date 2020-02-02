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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A schema extentation encapsulating subscriptions for a given schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class SchemaSubscriptionsExtension<TSchema> : ISchemaExtension
        where TSchema : class, ISchema
    {
        private SchemaOptions _primaryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaSubscriptionsExtension{TSchema}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SchemaSubscriptionsExtension(SchemaSubscriptionOptions<TSchema> options)
        {
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            this.RequiredServices = new HashSet<ServiceDescriptor>();
        }

        /// <summary>
        /// This method is called by the parent options just before it is added to the extensions
        /// collection. Use this method to do any sort of configuration, final default settings etc.
        /// This method represents the last opportunity for the extention options to modify its own required
        /// service collection before being incorporated with the DI container.
        /// </summary>
        /// <param name="options">The parent options which owns this extension.</param>
        public void Configure(SchemaOptions options)
        {
            _primaryOptions = options;
        }

        /// <summary>
        /// Invokes this instance to perform any final setup requirements as part of
        /// its configuration during startup.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
            // configure the subscription route middleware for invoking the graphql
            // pipeline for the subscription
            if (!this.Options.DisableDefaultRoute && app != null)
            {
                var routePath = this.Options.SubscriptionRoute.Replace(
                SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY,
                _primaryOptions.QueryHandler.Route);

                var middlewareType = this.Options.HttpProcessorType
                    ?? typeof(DefaultGraphQLHttpSubscriptionMiddleware<TSchema>);

                app.UseMiddleware(middlewareType, routePath);

                app.ApplicationServices.WriteLogEntry(
                      (l) => l.SchemaSubscriptionRouteRegistered<TSchema>(
                      routePath));
            }
        }

        /// <summary>
        /// Gets the options related to this extension instance.
        /// </summary>
        /// <value>The options.</value>
        public SchemaSubscriptionOptions<TSchema> Options { get; }

        /// <summary>
        /// Gets a collection of services this extension has registered that should be included in
        /// a DI container.
        /// </summary>
        /// <value>The additional types as formal descriptors.</value>
        public HashSet<ServiceDescriptor> RequiredServices { get; }
    }
}