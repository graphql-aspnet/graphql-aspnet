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
    using GraphQL.AspNet.Configuration;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// <para>
    /// An interface that can be used to configure custom extensions to a schema. An extension can be almost
    /// anthing that customizes the runtime for the target schema.
    /// </para>
    /// <para>
    /// For example, graphql subscriptions are implemented as a server extension.
    /// </para>
    /// </summary>
    public interface IGraphQLServerExtension
    {
        /// <summary>
        /// <para>
        /// This method is called by the schema configuration just before it is added to the extensions
        /// collection. Use this method to do any sort of internal configuration, default settings,
        /// additional DI container registrations etc.
        /// </para>
        /// <para>
        /// This method represents the last opportunity for this extension to modify its own required
        /// service collection before being incorporated with the DI container.
        /// </para>
        /// </summary>
        /// <param name="options">The schema options collection to which this extension
        /// is being registered.</param>
        void Configure(SchemaOptions options);

        /// <summary>
        /// Instructs this extension to perform any final setup requirements as part of
        /// its configuration during startup.
        /// </summary>
        /// <param name="app">The application builder to register against. May be <c>null</c> in some rare instances
        /// where the middleware pipeline is not being setup. Usually during some unit testing edge cases.</param>
        /// <param name="serviceProvider">The configured service provider completed during setup. In
        /// most instances, this will be the <see cref="IApplicationBuilder.ApplicationServices"/> instances
        /// from <paramref name="app"/>.</param>
        void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null);
    }
}