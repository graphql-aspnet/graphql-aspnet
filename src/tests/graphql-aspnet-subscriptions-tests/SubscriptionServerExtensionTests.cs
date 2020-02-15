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
    using GraphQL.AspNet.Middleware.SubscriptionEventExecution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionServerExtensionTests
    {
        private ISchemaPipelineBuilder<GraphSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> CreateEmptyBuilder()
        {
            var pipelineBuilder = new Mock<ISchemaPipelineBuilder<GraphSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext>>();
            pipelineBuilder.Setup(x => x.Build()).Returns(() =>
            {
                var pipeline = new Mock<ISchemaPipeline<GraphSchema, GraphSubscriptionExecutionContext>>();
                return pipeline.Object;
            });

            return pipelineBuilder.Object;
        }

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

            var extension = new SubscriptionServerExtension<GraphSchema>(subscriptionOptions, this.CreateEmptyBuilder());
            extension.Configure(primaryOptions);

            Assert.IsTrue(primaryOptions.DeclarationOptions.AllowedOperations.Contains(GraphCollection.Subscription));

            // 1. SchemaSubscriptionOptions
            // 2. execution pipeline
            Assert.AreEqual(2, extension.RequiredServices.Count);
            Assert.IsNotNull(extension.RequiredServices.SingleOrDefault(x => x.ServiceType == typeof(SubscriptionServerOptions<GraphSchema>)));
            Assert.IsNotNull(extension.RequiredServices.SingleOrDefault(x => x.ServiceType == typeof(ISchemaPipeline<GraphSchema, GraphSubscriptionExecutionContext>)));

            // Apollo:  Server, Client Factory, SubscriptionMaker, in-proc listener
            Assert.AreEqual(4, extension.OptionalServices.Count);
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionServer<GraphSchema>)));
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionClientFactory<GraphSchema>)));
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(IClientSubscriptionMaker<GraphSchema>)));
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionEventListener<GraphSchema>)));

            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
        }

        [Test]
        public void CustomMiddlewareComponent_WithoutProperSignature_throwsException()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            // invalid constructor
            subscriptionOptions.HttpMiddlewareComponentType = typeof(InvalidSubscriptionMiddlewareTester);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var extension = new SubscriptionServerExtension<GraphSchema>(subscriptionOptions, this.CreateEmptyBuilder());
                extension.Configure(primaryOptions);
            });
        }

        [Test]
        public void CustomMiddlewareComponent_WithProperSignature_Succeeds()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            // invalid constructor
            subscriptionOptions.HttpMiddlewareComponentType = typeof(ValidSubscriptionMiddlewareTester);

            // no exception should be thrown
            var extension = new SubscriptionServerExtension<GraphSchema>(subscriptionOptions, this.CreateEmptyBuilder());
            extension.Configure(primaryOptions);
        }
    }
}