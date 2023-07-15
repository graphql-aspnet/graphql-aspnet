// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.BackgroundServices;
    using GraphQL.AspNet.SubscriptionServer.Exceptions;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Configuration.ConfigurationTestData;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationSetupTests
    {
        [Test]
        public void AddSubscriptions_RegistrationChecks()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLSchemaBuilderExtensions.Clear();

            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
            })
            .AddSubscriptions();

            var sp = serviceCollection.BuildServiceProvider();

            // ensure both publishing and server stuff has been registered
            this.EnsureSubscriptionServerRegistrations(sp);
            this.EnsureSubscriptionPublishingRegistrations(sp);
        }

        [Test]
        public void ExplicitDeclarationOfPerFieldAuthorizationFailsServerCreation()
        {
            // setup the server with a hard declaration of nothing
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLSchemaBuilderExtensions.Clear();

            var serviceCollection = new ServiceCollection();
            var schemaBuilder = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
                options.AuthorizationOptions.Method = AuthorizationMethod.PerField;
            });

            // server should value to generate
            Assert.Throws<SubscriptionServerException>(
                () =>
                {
                    schemaBuilder.AddSubscriptionServer();
                });
        }

        [Test]
        public void ExplicitDeclarationOfPerRequestAuthorizationAddsServerSuccessfully()
        {
            // setup the server with a hard declaration of nothing
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLSchemaBuilderExtensions.Clear();

            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
                options.AuthorizationOptions.Method = AuthorizationMethod.PerRequest;
            })
            .AddSubscriptionServer();

            this.EnsureSubscriptionServerRegistrations(serviceCollection.BuildServiceProvider());
        }

        [Test]
        public void NonExplicitDeclarationResultsInPerRequestAndAddsServerSuccessfully()
        {
            // setup the server with a hard declaration of nothing
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            GraphQLSchemaBuilderExtensions.Clear();

            SchemaOptions<GraphSchema> optionsSaved = null;
            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
                options.AuthorizationOptions.Method = null;
                optionsSaved = options;
            })
            .AddSubscriptionServer();

            Assert.AreEqual(AuthorizationMethod.PerRequest, optionsSaved.AuthorizationOptions.Method);
            this.EnsureSubscriptionServerRegistrations(serviceCollection.BuildServiceProvider());
        }

        [Test]
        public void AddSubscriptionServer_RegistrationChecks()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLSchemaBuilderExtensions.Clear();

            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
            })
            .AddSubscriptionServer();

            this.EnsureSubscriptionServerRegistrations(serviceCollection.BuildServiceProvider());
        }

        private void EnsureSubscriptionServerRegistrations(IServiceProvider sp)
        {
            var controller = sp.GetService(typeof(FanController));
            Assert.IsNotNull(controller);

            // ensure schema operation type is/was allowed to be injected to the schema
            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);
            Assert.IsTrue(schema.Operations.ContainsKey(GraphOperationType.Subscription));

            // ensure router is registered
            Assert.IsNotNull(sp.GetService(typeof(ISubscriptionEventRouter)));
        }

        [Test]
        public void AddSubscriptionPublishing_RegistrationChecks()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            var serviceCollection = new ServiceCollection();

            // the internal publisher (added by default)
            // requires the router (1) which requires the client collection (2)
            // both of which are not "publishing specific"
            //
            // register this mock one to remove the dependency
            var externalPublisher = new Mock<ISubscriptionEventPublisher>();
            serviceCollection.AddSingleton(externalPublisher.Object);

            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
            })
            .AddSubscriptionPublishing();

            this.EnsureSubscriptionPublishingRegistrations(serviceCollection.BuildServiceProvider());
        }

        private void EnsureSubscriptionPublishingRegistrations(ServiceProvider sp)
        {
            var controller = sp.GetService(typeof(FanController));
            Assert.IsNotNull(controller);

            // ensure schema operation type is/was allowed to be injected to the schema
            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);
            Assert.IsTrue(schema.Operations.ContainsKey(GraphOperationType.Subscription));

            // ensure registered services for subscription server
            Assert.IsNotNull(sp.GetService(typeof(ISubscriptionEventPublisher)));
            Assert.IsNotNull(sp.GetService(typeof(SubscriptionEventPublishingQueue)));
            Assert.IsNotNull(sp.GetService(typeof(IHostedService)) as SubscriptionPublicationService);
        }
    }
}