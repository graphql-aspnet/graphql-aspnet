// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public static class TestServerSubscriptionHelpers
    {
        /// <summary>
        /// Adds subscription support to the test server builder.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the builder supports.</typeparam>
        /// <param name="serverBuilder">The server builder.</param>
        /// <param name="options">The options to configure the schema with.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        public static TestServerBuilder<TSchema> AddSubscriptions<TSchema>(
            this TestServerBuilder<TSchema> serverBuilder,
            Action<SchemaSubscriptionOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            var subOptions = new SchemaSubscriptionOptions<TSchema>();
            options?.Invoke(subOptions);
            serverBuilder.AddSchemaExtension(new SchemaSubscriptionsExtension<TSchema>(subOptions));

            return serverBuilder;
        }

        /// <summary>
        /// Retrieves the subscription server component from the test server.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the server is hosting.</typeparam>
        /// <param name="server">The test server.</param>
        /// <returns>ISubscriptionServer&lt;TSchema&gt;.</returns>
        public static ISubscriptionServer<TSchema> RetrieveSubscriptionServer<TSchema>(this TestServer<TSchema> server)
            where TSchema : class, ISchema
        {
            return server.ServiceProvider.GetService(typeof(ISubscriptionServer<TSchema>)) as ISubscriptionServer<TSchema>;
        }

        /// <summary>
        /// Creates a test client that mimics an always-connected websocket client.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the server is hosting.</typeparam>
        /// <param name="server">The server.</param>
        /// <returns>ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public static MockClientConnection CreateClient<TSchema>(this TestServer<TSchema> server)
            where TSchema : class, ISchema
        {
            return new MockClientConnection();
        }

        /// <summary>
        /// Creates a subscription client based on the DI setup of the test server.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema to create the client for.</typeparam>
        /// <param name="server">The server.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Subscriptions.ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public static (MockClientConnection, ISubscriptionClientProxy<TSchema>) CreateSubscriptionClient<TSchema>(this TestServer<TSchema> server)
                    where TSchema : class, ISchema
        {
            var factory = server.ServiceProvider.GetService<ISubscriptionClientFactory<TSchema>>();
            var options = server.ServiceProvider.GetService<SchemaSubscriptionOptions<TSchema>>();

            var context = new DefaultHttpContext();
            context.RequestServices = server.ServiceProvider;
            context.User = server.User;

            var connection = server.CreateClient();
            return (connection, factory.CreateClientProxy(context, connection, options));
        }
    }
}