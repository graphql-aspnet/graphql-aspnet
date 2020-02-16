// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Configuration
{
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Configuration.ConfigurationTestData;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationMvcSetupTests
    {
        [Test]
        public void AddSubscriptionServer_RegistrationChecks()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLProviders.TemplateProvider = new DefaultTypeTemplateProvider();
            GraphQLProviders.GraphTypeMakerProvider = new DefaultGraphTypeMakerProvider();
            GraphQLMvcSchemaBuilderExtensions.Clear();

            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<FanController>();
            })
            .AddSubscriptionServer();

            var sp = serviceCollection.BuildServiceProvider();
            var controller = sp.GetService(typeof(FanController));
            Assert.IsNotNull(controller);

            // ensure schema operation type is/was allowed to be injected to the schema
            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);
            Assert.IsTrue(schema.OperationTypes.ContainsKey(AspNet.Execution.GraphCollection.Subscription));

            // ensure registered services for subscription server
            Assert.IsNotNull(sp.GetService(typeof(ISubscriptionServer<GraphSchema>)));
            Assert.IsNotNull(sp.GetService(typeof(ISubscriptionClientFactory<GraphSchema>)));
            Assert.IsNotNull(sp.GetService(typeof(IClientSubscriptionMaker<GraphSchema>)));

            // ensure the template provider for the runtime is swapped
            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
            Assert.IsTrue(GraphQLProviders.GraphTypeMakerProvider is SubscriptionEnabledGraphTypeMakerProvider);
        }

        [Test]
        public void AddSubscriptionPublishing_RegistrationChecks()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLProviders.TemplateProvider = new DefaultTypeTemplateProvider();
            GraphQLProviders.GraphTypeMakerProvider = new DefaultGraphTypeMakerProvider();

            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<FanController>();
            })
            .AddSubscriptionPublishing();

            var sp = serviceCollection.BuildServiceProvider();
            var controller = sp.GetService(typeof(FanController));
            Assert.IsNotNull(controller);

            // ensure schema operation type is/was allowed to be injected to the schema
            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);
            Assert.IsTrue(schema.OperationTypes.ContainsKey(AspNet.Execution.GraphCollection.Subscription));

            // ensure registered services for subscription server
            Assert.IsNotNull(sp.GetService(typeof(ISubscriptionEventPublisher)));
            Assert.IsNotNull(sp.GetService(typeof(SubscriptionEventQueue)));
            Assert.IsNotNull(sp.GetService(typeof(IHostedService)) as SubscriptionPublicationService);

            // ensure the template provider for the runtime is swapped
            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
            Assert.IsTrue(GraphQLProviders.GraphTypeMakerProvider is SubscriptionEnabledGraphTypeMakerProvider);
        }
    }
}