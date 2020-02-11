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
    using System.Threading;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Http;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaExtensionTests
    {
        public class InvalidSubscriptionMiddlewareTester
        {
            public InvalidSubscriptionMiddlewareTester(
                RequestDelegate next,
                SchemaSubscriptionOptions<GraphSchema> options,
                string route,
                string other)
            {
            }
        }

        public class ValidSubscriptionMiddlewareTester
        {
            public ValidSubscriptionMiddlewareTester(
                RequestDelegate next,
                SchemaSubscriptionOptions<GraphSchema> options,
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
            var subscriptionOptions = new SchemaSubscriptionOptions<GraphSchema>();

            var extension = new SchemaSubscriptionsExtension<GraphSchema>(subscriptionOptions);
            extension.Configure(primaryOptions);

            Assert.IsTrue(primaryOptions.DeclarationOptions.AllowedOperations.Contains(GraphCollection.Subscription));

            // SchemaSubscriptionOptions
            Assert.AreEqual(1, extension.RequiredServices.Count);

            // Apollo:  Server, Client Factory, Supervisor, SubscriptionMaker
            Assert.AreEqual(4, extension.OptionalServices.Count);

            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
        }

        [Test]
        public void CustomMiddlewareComponent_WithoutProperSignature_throwsException()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SchemaSubscriptionOptions<GraphSchema>();

            // invalid constructor
            subscriptionOptions.HttpMiddlewareComponentType = typeof(InvalidSubscriptionMiddlewareTester);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var extension = new SchemaSubscriptionsExtension<GraphSchema>(subscriptionOptions);
                extension.Configure(primaryOptions);
            });
        }

        [Test]
        public void CustomMiddlewareComponent_WithProperSignature_Succeeds()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SchemaSubscriptionOptions<GraphSchema>();

            // invalid constructor
            subscriptionOptions.HttpMiddlewareComponentType = typeof(ValidSubscriptionMiddlewareTester);

            // no exception should be thrown
            var extension = new SchemaSubscriptionsExtension<GraphSchema>(subscriptionOptions);
            extension.Configure(primaryOptions);
        }
    }
}