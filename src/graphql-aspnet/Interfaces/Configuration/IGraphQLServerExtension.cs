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
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// <para>
    /// An object that can be used to configure custom extensions to a schema. An extension can be almost
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
        /// collection for the target schema. Use this method to do any sort of internal configuration, default settings,
        /// additional DI container registrations etc.
        /// </para>
        /// <para>
        /// This method represents the last opportunity for this extension to add its own required
        /// service collection registrations to the DI container.
        /// </para>
        /// </summary>
        /// <param name="options">The schema options representing the schema to which this extension
        /// is being registered.</param>
        public void Configure(SchemaOptions options)
        {
        }

        /// <summary>
        /// Instructs this extension to perform any final setup requirements before the server starts. This
        /// method is called as part of <c>.UseGraphQL()</c> during startup. All extensions are called in the order
        /// they are registered.
        /// </summary>
        /// <remarks>
        /// When this method is called, construction of the DI container is complete. The schema has not been
        /// generated yet.
        /// </remarks>
        /// <param name="app">The application builder to register against. May be <c>null</c> in some rare instances
        /// where the middleware pipeline is not being setup. Usually during some unit testing edge cases.</param>
        public void UseExtension(IApplicationBuilder app)
        {
            this.UseExtension(app, null);
        }

        /// <summary>
        /// Instructs this extension to perform any final setup requirements before the server starts. This
        /// method is called as part of <c>.UseGraphQL()</c> during startup. All extensions are called in the order
        /// they are registered.
        /// </summary>
        /// <remarks>
        /// When this method is called, construction of the DI container is complete. The schema has not been
        /// generated yet.
        /// </remarks>
        /// <param name="serviceProvider">The configured service provider completed during setup. In
        /// most instances, this will be the <see cref="IApplicationBuilder.ApplicationServices"/> instances
        /// from <paramref name="app"/>.</param>
        public void UseExtension(IServiceProvider serviceProvider)
        {
            this.UseExtension(null, serviceProvider);
        }

        /// <summary>
        /// Instructs this extension to perform any final setup requirements before the server starts. This
        /// method is called as part of <c>.UseGraphQL()</c> during startup. All extensions are called in the order
        /// they are registered.
        /// </summary>
        /// <remarks>
        /// When this method is called, construction of the DI container is complete. The schema has not been
        /// generated yet.
        /// </remarks>
        /// <param name="app">The application builder to register against. May be <c>null</c> in some rare instances
        /// where the middleware pipeline is not being setup. Usually during some unit testing edge cases.</param>
        /// <param name="serviceProvider">The configured service provider completed during setup. In
        /// most instances, this will be the <see cref="IApplicationBuilder.ApplicationServices"/> instances
        /// from <paramref name="app"/>.</param>
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
        }

        /// <summary>
        /// <para>
        /// Allows the extension the option to modify or inspect the integrity of a schema instance as its being built in order
        /// to apply any necessary logic or updates to the schema items that have been registered and parsed during setup.
        /// </para>
        /// <para>
        /// This method is invoked after all graph types and directives have been discovered and
        /// just before type system directives are applied.
        /// </para>
        /// </summary>
        /// <param name="schema">The schema to process.</param>
        public void EnsureSchema(ISchema schema)
        {
        }
    }
}