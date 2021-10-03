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
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class TestServerSubscriptionHelpers
    {
        /// <summary>
        /// Adds subscription server support to the test server builder.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the builder supports.</typeparam>
        /// <param name="serverBuilder">The server builder.</param>
        /// <param name="options">The options to configure the schema with.</param>
        /// <returns>TestServerBuilder&lt;TSchema&gt;.</returns>
        public static TestServerBuilder<TSchema> AddSubscriptionServer<TSchema>(
            this TestServerBuilder<TSchema> serverBuilder,
            Action<SubscriptionServerOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            var subscriptionsOptions = new SubscriptionServerOptions<TSchema>();
            options?.Invoke(subscriptionsOptions);

            serverBuilder.AddSchemaBuilderAction(schemaBuilder =>
            {
                DefaultSubscriptionBuilderExtensions
                .AddSubscriptionServer(schemaBuilder, options);
            });

            return serverBuilder;
        }

        /// <summary>
        /// Adds the ability for this test server to raise subscription events.
        /// </summary>
        /// <typeparam name="TSchema">The type of schema being configured.</typeparam>
        /// <param name="serverBuilder">The server builder.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Configuration.ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static TestServerBuilder<TSchema> AddSubscriptionPublishing<TSchema>(this TestServerBuilder<TSchema> serverBuilder)
            where TSchema : class, ISchema
        {
            serverBuilder.AddSchemaBuilderAction(builder =>
            {
                var extension = new SubscriptionPublisherSchemaExtension<TSchema>();
                builder.Options.RegisterExtension(extension);

                builder.QueryExecutionPipeline.AddMiddleware<PublishRaisedSubscriptionEventsMiddleware<TSchema>>();
                builder.AsServiceCollection().TryAddSingleton<SubscriptionPublicationService>();
                builder.AsServiceCollection().AddScoped<ISubscriptionEventPublisher, InProcessSubscriptionPublisher>();
            });

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
            return new MockClientConnection(
                server.ServiceProvider.CreateScope().ServiceProvider,
                server.User);
        }

        /// <summary>
        /// Creates a subscription client based on the DI setup of the test server.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema to create the client for.</typeparam>
        /// <param name="server">The server.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Subscriptions.ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public static async Task<(MockClientConnection, ISubscriptionClientProxy<TSchema>)> CreateSubscriptionClient<TSchema>(this TestServer<TSchema> server)
                    where TSchema : class, ISchema
        {
            var subServer = server.ServiceProvider.GetService<ISubscriptionServer<TSchema>>();
            var options = server.ServiceProvider.GetService<SubscriptionServerOptions<TSchema>>();

            var context = new DefaultHttpContext();
            var scope = server.ServiceProvider.CreateScope();
            context.RequestServices = scope.ServiceProvider;
            context.User = server.User;

            var connection = server.CreateClient();
            var subClient = await subServer.RegisterNewClient(connection) as ISubscriptionClientProxy<TSchema>;
            return (connection, subClient);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="SubscriptionServerOptions{TSchema}"/> from the test
        /// server's service provider.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema configured on the server.</typeparam>
        /// <param name="testServer">The test server.</param>
        /// <returns>SubscriptionServerOptions&lt;TSchema&gt;.</returns>
        public static SubscriptionServerOptions<TSchema> RetrieveSubscriptionServerOptions<TSchema>(this TestServer<TSchema> testServer)
            where TSchema : class, ISchema
        {
            return testServer.ServiceProvider.GetRequiredService<SubscriptionServerOptions<TSchema>>();
        }

        public static SubscriptionContextBuilder CreateSubcriptionContextBuilder<TSchema>(this TestServer<TSchema> testServer, ISubscriptionClientProxy client)
            where TSchema : class, ISchema
        {
            return new SubscriptionContextBuilder(client);
        }
    }
}