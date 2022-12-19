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
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Subscriptions.BackgroundServices;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.FieldExecution.Components;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionReceiverExtensionTests
    {
        private (
            Mock<ISchemaBuilder<GraphSchema>>,
            Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphQueryExecutionContext>, GraphQueryExecutionContext>>,
            Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphFieldExecutionContext>, GraphFieldExecutionContext>>)
            CreateSchemaBuilderMock(SchemaOptions<GraphSchema> options)
        {
            var queryPipeline = new Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphQueryExecutionContext>, GraphQueryExecutionContext>>();
            var fieldPipeline = new Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphFieldExecutionContext>, GraphFieldExecutionContext>>();

            var builder = new Mock<ISchemaBuilder<GraphSchema>>();
            builder.Setup(x => x.QueryExecutionPipeline).Returns(queryPipeline.Object);
            builder.Setup(x => x.FieldExecutionPipeline).Returns(fieldPipeline.Object);
            builder.Setup(x => x.Options).Returns(options);

            queryPipeline.Setup(x => x.Clear());
            queryPipeline.Setup(x => x.AddMiddleware<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(
                It.IsAny<ServiceLifetime>(),
                It.IsAny<string>())).Returns(queryPipeline.Object);

            queryPipeline.Setup(x => x.Clear());
            queryPipeline.Setup(x => x.AddMiddleware(
                It.IsAny<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(),
                It.IsAny<string>())).Returns(queryPipeline.Object);

            return (builder, queryPipeline, fieldPipeline);
        }

        [Test]
        public void ServiceCollection_VerifyDefaultInjectedObjects()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var serviceCollection = new ServiceCollection();
            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>(serviceCollection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            (var builder, var queryPipeline, var fieldPipeline) = CreateSchemaBuilderMock(primaryOptions);

            var extension = new SubscriptionReceiverSchemaExtension<GraphSchema>(builder.Object, subscriptionOptions);
            extension.Configure(primaryOptions);

            Assert.IsTrue(primaryOptions.DeclarationOptions.AllowedOperations.Contains(GraphOperationType.Subscription));

            Assert.AreEqual(8, primaryOptions.ServiceCollection.Count);

            // primary server objects
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ServiceType == typeof(SubscriptionServerOptions<GraphSchema>)));
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionServerClientFactory)));
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ServiceType == typeof(IGlobalSubscriptionClientProxyCollection)));
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionEventDispatchQueue)));
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ImplementationType == typeof(SubscriptionClientDispatchService)));

            // graphql-transport-ws objects
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ImplementationType == typeof(GqltwsSubscriptionClientProxyFactory)));

            // legacy graphql-ws objects
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ImplementationType == typeof(GraphqlWsLegacySubscriptionClientProxyFactory)));
            Assert.IsNotNull(primaryOptions.ServiceCollection.SingleOrDefault(x => x.ImplementationType == typeof(GraphqlWsLegacySubscriptionClientProxyFactoryAlternate)));

            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);
        }

        [Test]
        public void UseExtension_RegistersMiddlewareComponent()
        {
            var logger = new Mock<IGraphEventLogger>();
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IGraphEventLogger>(logger.Object);

            var primaryOptions = new SchemaOptions<GraphSchema>(serviceCollection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();
            subscriptionOptions.Route = "/graphql";

            (var builder, var queryPipeline, var fieldPipeline) = CreateSchemaBuilderMock(primaryOptions);

            var extension = new SubscriptionReceiverSchemaExtension<GraphSchema>(builder.Object, subscriptionOptions);
            extension.Configure(primaryOptions);

            var appBuilder = new Mock<IApplicationBuilder>();

            extension.UseExtension(appBuilder.Object, serviceCollection.BuildServiceProvider());

            appBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
            logger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<Func<IGraphLogEntry>>()), Times.Once);
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var serviceCollection = new ServiceCollection();
            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>(serviceCollection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            (var builder, var queryPipeline, var fieldPipeline) = CreateSchemaBuilderMock(primaryOptions);

            var extension = new SubscriptionReceiverSchemaExtension<GraphSchema>(builder.Object, subscriptionOptions);
            extension.Configure(primaryOptions);

            // 12 middleware components in the subscription-swapped primary query pipeline
            //    registered by type
            // 1 middleware component registered by instance
            queryPipeline.Verify(x => x.Clear());
            queryPipeline.Verify(
                x =>
                    x.AddMiddleware<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(
                            It.IsAny<ServiceLifetime>(),
                            It.IsAny<string>()),
                Times.Exactly(12));

            queryPipeline.Verify(
                x =>
                    x.AddMiddleware(
                        It.IsAny<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(),
                        It.IsAny<string>()),
                Times.Exactly(1));

            // ensure query level authorzation component was added
            queryPipeline.Verify(
              x =>
                  x.AddMiddleware<AuthorizeQueryOperationMiddleware<GraphSchema>>(
                          It.IsAny<ServiceLifetime>(),
                          It.IsAny<string>()),
              Times.Exactly(1));

            // original three components in the sub swaped field pipeline
            fieldPipeline.Verify(x => x.Clear());
            fieldPipeline.Verify(
                x =>
                    x.AddMiddleware<IGraphMiddlewareComponent<GraphFieldExecutionContext>>(It.IsAny<ServiceLifetime>(), It.IsAny<string>()),
                Times.Exactly(3));

            // ensure field authroization component was NOT added
            // to the field pipeline
            fieldPipeline.Verify(
                x =>
                    x.AddMiddleware<AuthorizeFieldMiddleware<GraphSchema>>(It.IsAny<ServiceLifetime>(), It.IsAny<string>()),
                Times.Exactly(0));
        }
    }
}