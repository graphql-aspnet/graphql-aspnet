// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// A schema extension encapsulating the ability for a given schema to publish subscription events from
    /// query and mutation operations.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this extension is built for.</typeparam>
    public sealed class SubscriptionPublisherSchemaExtension<TSchema> : IGraphQLServerExtension
        where TSchema : class, ISchema
    {
        private SchemaOptions _primaryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionPublisherSchemaExtension{TSchema}"/> class.
        /// </summary>
        public SubscriptionPublisherSchemaExtension()
        {
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
            _primaryOptions.DeclarationOptions.AllowedOperations.Add(GraphOperationType.Subscription);

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
    }
}