// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests
{
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionPublisherExtensionTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            GraphQLProviders.TemplateProvider = null;
            var collection = new ServiceCollection();

            var primaryOptions = new SchemaOptions<GraphSchema>(collection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            var extension = new SubscriptionPublisherSchemaExtension<GraphSchema>();
            extension.Configure(primaryOptions);

            Assert.IsTrue(primaryOptions.DeclarationOptions.AllowedOperations.Contains(GraphOperationType.Subscription));
            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
        }
    }
}