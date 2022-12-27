// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Mocks
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.BackgroundServices;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Tests.Mocks;
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
        public static ITestServerBuilder<TSchema> AddSubscriptionServer<TSchema>(
            this ITestServerBuilder<TSchema> serverBuilder,
            Action<SubscriptionServerOptions<TSchema>> options = null)
            where TSchema : class, ISchema
        {
            var subscriptionsOptions = new SubscriptionServerOptions<TSchema>();
            options?.Invoke(subscriptionsOptions);

            serverBuilder.AddSchemaBuilderAction(schemaBuilder =>
            {
                schemaBuilder.AddSubscriptionServer(options);
            });

            return serverBuilder;
        }

        /// <summary>
        /// Adds the ability for this test server to raise subscription events.
        /// </summary>
        /// <typeparam name="TSchema">The type of schema being configured.</typeparam>
        /// <param name="serverBuilder">The server builder.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Configuration.ISchemaBuilder&lt;TSchema&gt;.</returns>
        public static ITestServerBuilder<TSchema> AddSubscriptionPublishing<TSchema>(this ITestServerBuilder<TSchema> serverBuilder)
            where TSchema : class, ISchema
        {
            serverBuilder.AddSchemaBuilderAction(builder =>
            {
                var extension = new SubscriptionPublisherSchemaExtension<TSchema>();
                builder.Options.RegisterExtension(extension);

                builder.QueryExecutionPipeline.AddMiddleware<PublishRaisedSubscriptionEventsMiddleware<TSchema>>();
                builder.Options.ServiceCollection.TryAddSingleton<SubscriptionPublicationService>();
                builder.Options.ServiceCollection.AddScoped<ISubscriptionEventPublisher, InProcessSubscriptionPublisher>();
            });

            return serverBuilder;
        }

        /// <summary>
        /// Creates a test client that mimics an always-connected websocket client.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the server is hosting.</typeparam>
        /// <param name="server">The server.</param>
        /// <param name="requestedProtocol">The requested protocol to assign to the connection, mimicing
        /// a value parsed from a websocket connection.</param>
        /// <returns>ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public static MockClientConnection CreateClientConnection<TSchema>(this TestServer<TSchema> server, string requestedProtocol = "")
            where TSchema : class, ISchema
        {
            return new MockClientConnection(
                server.ServiceProvider.CreateScope().ServiceProvider,
                server.SecurityContext,
                requestedProtocol: requestedProtocol);
        }

        /// <summary>
        /// Creates a subscription client based on the DI setup of the test server.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema to create the client for.</typeparam>
        /// <param name="server">The server.</param>
        /// <param name="stateOfConnection">The state of underlying connection that the client should report.</param>
        /// <returns>ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public static (MockSubscriptionClientProxy<TSchema> Client, IServiceProvider ServiceProvider, IUserSecurityContext SecurityContext) CreateSubscriptionClient<TSchema>(
            this TestServer<TSchema> server,
            ClientConnectionState stateOfConnection = ClientConnectionState.Open)
                    where TSchema : class, ISchema
        {
            var options = server.ServiceProvider.GetService<SubscriptionServerOptions<TSchema>>();

            var serviceProvider = server.ServiceProvider.CreateScope().ServiceProvider;

            var client = new MockSubscriptionClientProxy<TSchema>(
                serviceProvider,
                server.SecurityContext,
                stateOfConnection);

            return (client, serviceProvider, server.SecurityContext);
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

        public static SubscriptionContextBuilder CreateSubcriptionContextBuilder<TSchema>(
            this TestServer<TSchema> testServer,
            ISubscriptionClientProxy client,
            IServiceProvider serviceProvider,
            IUserSecurityContext securityContext)
            where TSchema : class, ISchema
        {
            return new SubscriptionContextBuilder(client, serviceProvider, securityContext);
        }
    }
}