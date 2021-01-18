// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An interface that can be used to configure extensions to a schema.
    /// </summary>
    public interface ISchemaExtension
    {
        /// <summary>
        /// This method is called by the parent options just before it is added to the extensions
        /// collection. Use this method to do any sort of configuration, final default settings etc.
        /// This method represents the last opportunity for the extention options to modify its own required
        /// service collection before being incorporated with the DI container.
        /// </summary>
        /// <param name="options">The parent options which owns this extension.</param>
        void Configure(SchemaOptions options);

        /// <summary>
        /// Invokes this instance to perform any final setup requirements as part of
        /// its configuration during startup.
        /// </summary>
        /// <param name="app">The application builder, no middleware will be registered if not supplied.</param>
        /// <param name="serviceProvider">The service provider to use. </param>
        void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null);

        /// <summary>
        /// Gets a collection of services this extension has registered that should be included in
        /// a DI container.
        /// </summary>
        /// <value>The additional types as formal descriptors.</value>
        List<ServiceDescriptor> RequiredServices { get; }

        /// <summary>
        /// Gets a collection of services this extension has registered that may be included in
        /// a DI container. If they cannot be added, because a reference already exists, they will be skipped.
        /// </summary>
        /// <value>The additional types as formal descriptors.</value>
        List<ServiceDescriptor> OptionalServices { get; }
    }
}