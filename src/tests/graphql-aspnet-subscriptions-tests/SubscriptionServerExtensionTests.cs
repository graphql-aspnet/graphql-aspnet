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
    using System;
    using System.Linq;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionServerExtensionTests
    {
        public class InvalidSubscriptionMiddlewareTester
        {
            public InvalidSubscriptionMiddlewareTester(
                RequestDelegate next,
                SubscriptionServerOptions<GraphSchema> options,
                string route,
                string other)
            {
            }
        }

        public class ValidSubscriptionMiddlewareTester
        {
            public ValidSubscriptionMiddlewareTester(
                RequestDelegate next,
                SubscriptionServerOptions<GraphSchema> options,
                string route)
            {
            }
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            var builder = new Mock<ISchemaBuilder<GraphSchema>>();

            var extension = new SubscriptionServerSchemaExtension<GraphSchema>(builder.Object, subscriptionOptions);
            extension.Configure(primaryOptions);

            Assert.IsTrue(primaryOptions.DeclarationOptions.AllowedOperations.Contains(GraphCollection.Subscription));

            Assert.AreEqual(1, extension.RequiredServices.Count);
            Assert.IsNotNull(extension.RequiredServices.SingleOrDefault(x => x.ServiceType == typeof(SubscriptionServerOptions<GraphSchema>)));

            Assert.AreEqual(3, extension.OptionalServices.Count);
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionServer<GraphSchema>)));
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionClientFactory<GraphSchema>)));
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(IClientSubscriptionMaker<GraphSchema>)));

            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
        }

        [Test]
        public void CustomHttpMiddlewareComponent_WithoutProperConstructor_throwsException()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            // invalid constructor
            subscriptionOptions.HttpMiddlewareComponentType = typeof(InvalidSubscriptionMiddlewareTester);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var mock = new Mock<ISchemaBuilder<GraphSchema>>();
                var extension = new SubscriptionServerSchemaExtension<GraphSchema>(mock.Object, subscriptionOptions);
                extension.Configure(primaryOptions);
            });
        }

        [Test]
        public void CustomHttpMiddlewareComponent_WithProperConstructor_Succeeds()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            // invalid constructor
            subscriptionOptions.HttpMiddlewareComponentType = typeof(ValidSubscriptionMiddlewareTester);

            var mock = new Mock<ISchemaBuilder<GraphSchema>>();

            // no exception should be thrown
            var extension = new SubscriptionServerSchemaExtension<GraphSchema>(mock.Object, subscriptionOptions);
            extension.Configure(primaryOptions);
        }
    }
}