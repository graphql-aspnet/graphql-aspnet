// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ContextTests
    {
        [Test]
        public void GraphFieldExecutionContext_PropertyCheck()
        {
            var parentContext = Substitute.For<IGraphQLMiddlewareExecutionContext>();

            var queryRequest = Substitute.For<IQueryExecutionRequest>();
            var variableData = Substitute.For<IResolvedVariableCollection>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var logger = Substitute.For<IGraphEventLogger>();
            var metrics = Substitute.For<IQueryExecutionMetrics>();
            var securityContext = Substitute.For<IUserSecurityContext>();

            parentContext.QueryRequest.Returns(queryRequest);
            parentContext.ServiceProvider.Returns(serviceProvider);
            parentContext.SecurityContext.Returns(securityContext);
            parentContext.Metrics.Returns(metrics);
            parentContext.Logger.Returns(logger);
            parentContext.Session.Returns(new QuerySession());

            var fieldRequest = Substitute.For<IGraphFieldRequest>();
            var sourceFieldCollection = new FieldSourceCollection();

            var context = new GraphFieldExecutionContext(
                parentContext,
                fieldRequest,
                variableData,
                sourceFieldCollection);

            Assert.AreEqual(queryRequest, context.QueryRequest);
            Assert.AreEqual(variableData, context.VariableData);
            Assert.AreEqual(sourceFieldCollection, context.DefaultFieldSources);
            Assert.AreEqual(fieldRequest, context.Request);
            Assert.IsEmpty(context.ResolvedSourceItems);
            Assert.AreEqual(serviceProvider, context.ServiceProvider);
            Assert.AreEqual(logger, context.Logger);
            Assert.AreEqual(metrics, context.Metrics);
            Assert.AreEqual(securityContext, context.SecurityContext);
        }

        [Test]
        public void GraphQueryExecutionContext_PropertyCheck()
        {
            var parentContext = Substitute.For<IGraphQLMiddlewareExecutionContext>();

            var queryRequest = Substitute.For<IQueryExecutionRequest>();
            var variableData = Substitute.For<IResolvedVariableCollection>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var logger = Substitute.For<IGraphEventLogger>();
            var metrics = Substitute.For<IQueryExecutionMetrics>();
            var securityContext = Substitute.For<IUserSecurityContext>();

            var fieldRequest = Substitute.For<IGraphFieldRequest>();
            var sourceFieldCollection = new FieldSourceCollection();

            var session = new QuerySession();
            var items = new MetaDataCollection();

            var context = new QueryExecutionContext(
                queryRequest,
                serviceProvider,
                session,
                items,
                securityContext,
                metrics,
                logger);

            Assert.AreEqual(queryRequest, context.QueryRequest);
            Assert.AreEqual(sourceFieldCollection, context.DefaultFieldSources);
            Assert.AreEqual(serviceProvider, context.ServiceProvider);
            Assert.AreEqual(logger, context.Logger);
            Assert.AreEqual(metrics, context.Metrics);
            Assert.AreEqual(securityContext, context.SecurityContext);
            Assert.AreEqual(session, context.Session);
            Assert.AreEqual(items, context.Items);
            Assert.IsNotNull(context.DefaultFieldSources);
            Assert.IsNotNull(context.FieldResults);
            Assert.IsNull(context.Result);
        }

        [Test]
        public void GraphDirectiveExecutionContext_PropertyCheck()
        {
            var parentContext = Substitute.For<IGraphQLMiddlewareExecutionContext>();

            var queryRequest = Substitute.For<IQueryExecutionRequest>();
            var variableData = Substitute.For<IResolvedVariableCollection>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var logger = Substitute.For<IGraphEventLogger>();
            var metrics = Substitute.For<IQueryExecutionMetrics>();
            var securityContext = Substitute.For<IUserSecurityContext>();
            var schema = new GraphSchema();
            var user = new ClaimsPrincipal();
            var session = new QuerySession();
            var messages = new GraphMessageCollection();

            parentContext.QueryRequest.Returns(queryRequest);
            parentContext.ServiceProvider.Returns(serviceProvider);
            parentContext.SecurityContext.Returns(securityContext);
            parentContext.Metrics.Returns(metrics);
            parentContext.Logger.Returns(logger);
            parentContext.Session.Returns(new QuerySession());

            var args = Substitute.For<IExecutionArgumentCollection>();
            var directiveRequest = Substitute.For<IGraphDirectiveRequest>();
            var sourceFieldCollection = new FieldSourceCollection();

            var context = new DirectiveResolutionContext(
                serviceProvider,
                session,
                schema,
                queryRequest,
                directiveRequest,
                args,
                messages,
                logger,
                user);

            Assert.AreEqual(queryRequest, context.QueryRequest);
            Assert.AreEqual(directiveRequest, context.Request);
            Assert.AreEqual(serviceProvider, context.ServiceProvider);
            Assert.AreEqual(logger, context.Logger);
            Assert.AreEqual(schema, context.Schema);
            Assert.AreEqual(directiveRequest, context.Request);
            Assert.AreEqual(messages, context.Messages);
            Assert.AreEqual(session, context.Session);
        }
    }
}