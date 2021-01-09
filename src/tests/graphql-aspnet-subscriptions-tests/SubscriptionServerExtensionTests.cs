﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests
{
    using System.Linq;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionServerExtensionTests
    {
        private (Mock<ISchemaBuilder<GraphSchema>>, Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphQueryExecutionContext>, GraphQueryExecutionContext>>)
            CreateSchemaBuilderMock()
        {
            var queryPipeline = new Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphQueryExecutionContext>, GraphQueryExecutionContext>>();
            var fieldPipeline = new Mock<ISchemaPipelineBuilder<GraphSchema, IGraphMiddlewareComponent<GraphFieldExecutionContext>, GraphFieldExecutionContext>>();

            var builder = new Mock<ISchemaBuilder<GraphSchema>>();
            builder.Setup(x => x.QueryExecutionPipeline).Returns(queryPipeline.Object);
            builder.Setup(x => x.FieldExecutionPipeline).Returns(fieldPipeline.Object);

            queryPipeline.Setup(x => x.Clear());
            queryPipeline.Setup(x => x.AddMiddleware<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(
                It.IsAny<ServiceLifetime>(),
                It.IsAny<string>())).Returns(queryPipeline.Object);

            queryPipeline.Setup(x => x.Clear());
            queryPipeline.Setup(x => x.AddMiddleware(
                It.IsAny<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(),
                It.IsAny<string>())).Returns(queryPipeline.Object);

            return (builder, queryPipeline);
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>();
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            (var builder, var queryPipeline) = CreateSchemaBuilderMock();

            var extension = new ApolloSubscriptionServerSchemaExtension<GraphSchema>(builder.Object, subscriptionOptions);
            extension.Configure(primaryOptions);

            Assert.IsTrue(primaryOptions.DeclarationOptions.AllowedOperations.Contains(GraphCollection.Subscription));

            Assert.AreEqual(2, extension.RequiredServices.Count);
            Assert.IsNotNull(extension.RequiredServices.SingleOrDefault(x => x.ServiceType == typeof(SubscriptionServerOptions<GraphSchema>)));
            Assert.IsNotNull(extension.RequiredServices.SingleOrDefault(x => x.ServiceType == typeof(ApolloMessageConverterFactory)));

            Assert.AreEqual(1, extension.OptionalServices.Count);
            Assert.IsNotNull(extension.OptionalServices.SingleOrDefault(x => x.ServiceType == typeof(ISubscriptionServer<GraphSchema>)));

            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTemplateProvider);

            // 8 middleware components in the subscription-swapped primary query pipeline registered by type
            // 1 middleware component registered by instance
            queryPipeline.Verify(x => x.Clear());
            queryPipeline.Verify(
                x =>
                    x.AddMiddleware<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(It.IsAny<ServiceLifetime>(), It.IsAny<string>()),
                Times.Exactly(8));

            queryPipeline.Verify(
                x =>
                    x.AddMiddleware(
                    It.IsAny<IGraphMiddlewareComponent<GraphQueryExecutionContext>>(),
                    It.IsAny<string>()),
                Times.Exactly(1));
        }
    }
}