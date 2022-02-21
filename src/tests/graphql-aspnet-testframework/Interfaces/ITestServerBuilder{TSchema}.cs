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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A test server builder targeting a specific schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema being targetd.</typeparam>
    public interface ITestServerBuilder<TSchema> : ITestServerBuilder
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Adds the test component to those injected into the service provider when the server is built.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddTestComponent(IGraphTestFrameworkComponent component);

        /// <summary>
        /// Helpful overload for direct access to inject a graph type into the server.
        /// </summary>
        /// <typeparam name="TType">The concrete type to parse when creating a graph type.</typeparam>
        /// <returns>TestServerBuilder.</returns>
        ITestServerBuilder<TSchema> AddGraphType<TType>();

        /// <summary>
        /// Helpful overload for direct access to inject a graph type into the server.
        /// </summary>
        /// <param name="type">The type to inject.</param>
        /// <returns>TestServerBuilder.</returns>
        ITestServerBuilder<TSchema> AddGraphType(Type type);

        /// <summary>
        /// Adds the graph controller to the server and renders it and its dependents into the target schema.
        /// </summary>
        /// <typeparam name="TController">The type of the t controller.</typeparam>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddGraphController<TController>()
            where TController : GraphController;

        /// <summary>
        /// Provides access to the options configuration function used when graphQL is first added
        /// to a server instance.
        /// </summary>
        /// <param name="configureOptions">The function to invoke to configure graphql settings.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddGraphQL(Action<SchemaOptions> configureOptions);

        /// <summary>
        /// Adds an action to execute against the master schema builder when this server is built. Mimics a
        /// call to <see cref="AddGraphQL(Action{SchemaOptions})"/> then taking further action against
        /// the returned <see cref="ISchemaBuilder{TSchema}"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>GraphQL.AspNet.Tests.Framework.TestServerBuilder&lt;TSchema&gt;.</returns>
        ITestServerBuilder<TSchema> AddSchemaBuilderAction(Action<ISchemaBuilder<TSchema>> action);

        /// <summary>
        /// Creates a new test server instance from the current settings in this builder.
        /// </summary>
        /// <returns>TestServer.</returns>
        TestServer<TSchema> Build();

        /// <summary>
        /// Gets the schema options containing all the configuration data for the schema this test
        /// server will execute for.
        /// </summary>
        /// <value>The schema options.</value>
        SchemaOptions<TSchema> SchemaOptions { get; }
    }
}