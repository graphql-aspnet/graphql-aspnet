// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Framework.Interfaces
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A test server builder targeting a specific schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema being targetd.</typeparam>
    public interface ITestServerBuilder<TSchema> : ITestServerBuilder, IServiceCollection
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Adds the custom testing component into the service provider when the server is built.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddTestComponent(IGraphQLTestFrameworkComponent component);

        /// <inheritdoc cref="SchemaOptions.AddGraphType{TType}(TypeKind?)" />
        ITestServerBuilder<TSchema> AddGraphType<TType>(TypeKind? typeKind = null);

        /// <inheritdoc cref="SchemaOptions.AddGraphType(Type, TypeKind?)" />
        ITestServerBuilder<TSchema> AddGraphType(Type type, TypeKind? typeKind = null);

        /// <inheritdoc cref="SchemaOptions.AddType{TType}(TypeKind?)" />
        ITestServerBuilder<TSchema> AddType<TType>(TypeKind? typeKind = null);

        /// <inheritdoc cref="SchemaOptions.AddType(Type, TypeKind?)" />
        ITestServerBuilder<TSchema> AddType(Type type, TypeKind? typeKind = null);

        /// <inheritdoc cref="SchemaOptions.AddDirective{TDirective}(ServiceLifetime?)" />
        ITestServerBuilder<TSchema> AddDirective<TType>(ServiceLifetime? customLifetime = null)
            where TType : GraphDirective;

        /// <inheritdoc cref="SchemaOptions.AddController{TController}(ServiceLifetime?)" />
        ITestServerBuilder<TSchema> AddController<TController>(ServiceLifetime? customLifetime = null)
            where TController : GraphController;

        /// <inheritdoc cref="SchemaOptions.AddController{TController}(ServiceLifetime?)" />
        ITestServerBuilder<TSchema> AddGraphController<TController>(ServiceLifetime? customLifetime = null)
            where TController : GraphController;

        /// <summary>
        /// Provides access to set the runtime configuration function used when graphQL is first added
        /// to a server instance.
        /// </summary>
        /// <param name="configureOptions">The function to invoke to configure graphql settings.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddGraphQL(Action<SchemaOptions> configureOptions);

        /// <summary>
        /// Adds a custom action to execute against the master schema builder when this server
        /// is built. Mimics a scenario where you would call <c>.AddGraphQL()</c> then perform
        /// further actions against the returned <see cref="ISchemaBuilder{TSchema}"/>, such as updating
        /// middleware pipelines.
        /// </summary>
        /// <param name="action">The action to perform against the schema builder.</param>
        /// <returns>GraphQL.AspNet.Tests.Framework.TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddSchemaBuilderAction(Action<ISchemaBuilder<TSchema>> action);

        /// <summary>
        /// Builds a new test server instance from the current settings in this builder.
        /// </summary>
        /// <returns>TestServer.</returns>
        TestServer<TSchema> Build();

        /// <summary>
        /// Gets the schema options containing all the configuration data for the target schema.
        /// </summary>
        /// <value>The schema options.</value>
        SchemaOptions<TSchema> SchemaOptions { get; }
    }
}