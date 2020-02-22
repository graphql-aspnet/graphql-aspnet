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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A schema extension encapsulating the ability for a given schema to publish subscription events from
    /// query and mutation operations.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this extension is built for.</typeparam>
    public class SubscriptionPublisherSchemaExtension<TSchema> : ISchemaExtension
        where TSchema : class, ISchema
    {
        private SchemaOptions _primaryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionPublisherSchemaExtension{TSchema}"/> class.
        /// </summary>
        public SubscriptionPublisherSchemaExtension()
        {
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
        }

        /// <summary>
        /// Invokes this instance to perform any final setup requirements as part of
        /// its configuration during startup.
        /// </summary>
        /// <param name="app">The application builder, no middleware will be registered if not supplied.</param>
        /// <param name="serviceProvider">The service provider to use.</param>
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
        }

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