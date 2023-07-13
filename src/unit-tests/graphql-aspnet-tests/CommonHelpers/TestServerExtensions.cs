// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Tests.Framework;

    public static class TestServerExtensions
    {
        /// <summary>
        /// Creates a new maker factory that the test server can use.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema to render the factory for.</typeparam>
        /// <param name="serverInstance">The server instance.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Engine.IGraphQLTypeMakerFactory&lt;TSchema&gt;.</returns>
        public static IGraphQLTypeMakerFactory<TSchema> CreateMakerFactory<TSchema>(this TestServer<TSchema> serverInstance)
            where TSchema : class, ISchema
        {
            var factory = new DefaultGraphQLTypeMakerFactory<TSchema>();
            factory.Initialize(serverInstance.Schema);

            return factory;
        }
    }
}