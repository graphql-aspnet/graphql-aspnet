// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscrptions.Tests.Configuration
{
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Middleware.FieldAuthorization;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.ThirdPartyDll;
    using GraphQL.AspNet.Tests.ThirdPartyDll.Model;
    using GraphQL.Subscrptions.Tests.Configuration.ConfigurationTestData;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationMvcSetupTests
    {
        [Test]
        public void AddGraphQL_AddingDefaultSchema_WithOneSubscriptionController_GeneratesAllDefaultEngineParts()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            // ensure the runtime is in a default state (just in case the statics got messed up)
            GraphQLProviders.TemplateProvider = new DefaultTypeTemplateProvider();

            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<FanController>();
            })
            .AddSubscriptions();

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

            // ensure additional required apollo things are added
            Assert.IsNotNull(sp.GetService(typeof(ApolloClientSupervisor<GraphSchema>)));

            // ensure the template provider for the runtime is swapped
            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
        }
    }
}