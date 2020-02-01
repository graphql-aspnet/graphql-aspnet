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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configuration options relating to subscriptions for a given schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class SchemaSubscriptionOptions<TSchema> : ISchemaOptionsExtension
        where TSchema : class, ISchema
    {
        private Type _subscriptionProcessorType = null;
        private SchemaOptions _options = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaSubscriptionOptions{TSchema}"/> class.
        /// </summary>
        public SchemaSubscriptionOptions()
        {
            this.RequiredServices = new HashSet<ServiceDescriptor>();
        }

        /// <summary>
        /// Gets or sets the route path where any subscription clients should connect. use <see cref="SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY"/>
        /// as part of your route to represent the primary configured route for this schema. (Default: "{schemaRoute}/subscriptions").
        /// </summary>
        /// <value>The route.</value>
        public string SubscriptionRoute { get; set; } = SubscriptionConstants.Routing.DEFAULT_SUBSCRIPTIONS_ROUTE;

        /// <summary>
        /// <para>Gets or sets an optional .NET type to use as the processor for Subscription requests. When set,
        /// this type must inherit from <see cref="IGraphQLHttpSubscriptionProcessor{TSchema}" />.(Default: null).
        /// </para>
        /// <para>It can be advantagous to override the default  <see cref="DefaultGraphQLHttpSubscriptionProcessor{TSchema}" /> and register your custom
        /// type here rather than overriding the entire subscription middleware component. See the documentation for further details.</para>
        /// </summary>
        /// <value>The type of the processor to use when processing WebSocket Requests received by the application for subscriptions.</value>
        public Type HttpProcessorType
        {
            get
            {
                return _subscriptionProcessorType;
            }

            set
            {
                Validation.ThrowIfNotCastable<IGraphQLHttpSubscriptionProcessor<TSchema>>(value, nameof(HttpProcessorType));
                _subscriptionProcessorType = value;
                this.RequiredServices.Add(new ServiceDescriptor(
                    typeof(IGraphQLHttpSubscriptionProcessor<TSchema>),
                    _subscriptionProcessorType,
                    ServiceLifetime.Scoped));
            }
        }

        /// <summary>
        /// Gets a collection of services this extension has registered that should be included in
        /// a DI container.
        /// </summary>
        /// <value>The additional types as formal descriptors.</value>
        public HashSet<ServiceDescriptor> RequiredServices { get; }

        /// <summary>
        /// This method is called by the parent options just before it is added to the extensions
        /// collection. Use this method to do any sort of configuration, final default settings etc.
        /// This method represents the last opportunity for the extention options to modify its own required
        /// service collection before being incorporated with the DI container.
        /// </summary>
        /// <param name="options">The parent options which owns this extension.</param>
        public void Configure(SchemaOptions options)
        {
            _options = options;
            if (this.HttpProcessorType == null)
                this.HttpProcessorType = typeof(DefaultGraphQLHttpSubscriptionProcessor<TSchema>);
        }

        /// <summary>
        /// Invokes this instance to perform any final setup requirements as part of
        /// its configuration during startup.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
            // configure the subscription route and handler for invoking the graphql
            // pipeline for the subscription

        }
    }
}