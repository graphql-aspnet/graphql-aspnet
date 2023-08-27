// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests
{
    using System;
    using System.Linq;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Middleware.FieldExecution.Components;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.SubscriptionServer.BackgroundServices;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionReceiverExtensionTests
    {
        private (
            ISchemaBuilder<GraphSchema>,
            ISchemaPipelineBuilder<GraphSchema, IGraphQLMiddlewareComponent<QueryExecutionContext>, QueryExecutionContext>,
            ISchemaPipelineBuilder<GraphSchema, IGraphQLMiddlewareComponent<GraphFieldExecutionContext>, GraphFieldExecutionContext>)
            CreateSchemaBuilderMock(SchemaOptions<GraphSchema> options)
        {
            var queryPipeline = Substitute.For<ISchemaPipelineBuilder<GraphSchema, IGraphQLMiddlewareComponent<QueryExecutionContext>, QueryExecutionContext>>();
            var fieldPipeline = Substitute.For<ISchemaPipelineBuilder<GraphSchema, IGraphQLMiddlewareComponent<GraphFieldExecutionContext>, GraphFieldExecutionContext>>();

            var builder = Substitute.For<ISchemaBuilder<GraphSchema>>();
            builder.QueryExecutionPipeline.Returns(queryPipeline);
            builder.FieldExecutionPipeline.Returns(fieldPipeline);
            builder.Options.Returns(options);

            queryPipeline.Clear();
            queryPipeline.AddMiddleware<IGraphQLMiddlewareComponent<QueryExecutionContext>>(
                Arg.Any<ServiceLifetime>(),
                Arg.Any<string>()).Returns(queryPipeline);

            queryPipeline.Clear();
            queryPipeline.AddMiddleware(
                Arg.Any<IGraphQLMiddlewareComponent<QueryExecutionContext>>(),
                Arg.Any<string>()).Returns(queryPipeline);

            return (builder, queryPipeline, fieldPipeline);
        }

        [Test]
        public void ServiceCollection_VerifyDefaultInjectedObjects()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            var serviceCollection = new ServiceCollection();
            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>(serviceCollection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            (var builder, var queryPipeline, var fieldPipeline) = CreateSchemaBuilderMock(primaryOptions);

            var extension = new SubscriptionReceiverSchemaExtension<GraphSchema>(builder, subscriptionOptions);
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

            Assert.IsTrue(GraphQLProviders.TemplateProvider is SubscriptionEnabledTypeTemplateProvider);
        }

        [Test]
        public void UseExtension_RegistersMiddlewareComponent()
        {
            var logger = Substitute.For<IGraphEventLogger>();
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IGraphEventLogger>(logger);

            var primaryOptions = new SchemaOptions<GraphSchema>(serviceCollection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();
            subscriptionOptions.Route = "/graphql";

            (var builder, var queryPipeline, var fieldPipeline) = CreateSchemaBuilderMock(primaryOptions);

            var extension = new SubscriptionReceiverSchemaExtension<GraphSchema>(builder, subscriptionOptions);
            extension.Configure(primaryOptions);

            var appBuilder = Substitute.For<IApplicationBuilder>();

            extension.UseExtension(appBuilder, serviceCollection.BuildServiceProvider());

            appBuilder.Received(1).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
            logger.Received(1).Log(Arg.Any<LogLevel>(), Arg.Any<Func<IGraphLogEntry>>());
        }

        [Test]
        public void GeneralPropertyCheck()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();

            var serviceCollection = new ServiceCollection();
            GraphQLProviders.TemplateProvider = null;

            var primaryOptions = new SchemaOptions<GraphSchema>(serviceCollection);
            var subscriptionOptions = new SubscriptionServerOptions<GraphSchema>();

            (var builder, var queryPipeline, var fieldPipeline) = CreateSchemaBuilderMock(primaryOptions);

            var extension = new SubscriptionReceiverSchemaExtension<GraphSchema>(builder, subscriptionOptions);
            extension.Configure(primaryOptions);

            // 12 middleware components in the subscription-swapped primary query pipeline
            //    registered by type
            // 1 middleware component registered by instance
            queryPipeline.Received().Clear();
            queryPipeline.Received(12).AddMiddleware<IGraphQLMiddlewareComponent<QueryExecutionContext>>(
                            Arg.Any<ServiceLifetime>(),
                            Arg.Any<string>());

            queryPipeline.Received(1).AddMiddleware(
                        Arg.Any<IGraphQLMiddlewareComponent<QueryExecutionContext>>(),
                        Arg.Any<string>());

            // ensure query level authorzation component was added
            queryPipeline.Received(1).AddMiddleware<AuthorizeQueryOperationMiddleware<GraphSchema>>(
                          Arg.Any<ServiceLifetime>(),
                          Arg.Any<string>());

            // original three components in the sub swaped field pipeline
            fieldPipeline.Received().Clear();
            fieldPipeline.Received(3).AddMiddleware<IGraphQLMiddlewareComponent<GraphFieldExecutionContext>>(Arg.Any<ServiceLifetime>(), Arg.Any<string>());

            // ensure field authroization component was NOT added
            // to the field pipeline
            fieldPipeline.Received(0).AddMiddleware<AuthorizeFieldMiddleware<GraphSchema>>(Arg.Any<ServiceLifetime>(), Arg.Any<string>());
        }
    }
}