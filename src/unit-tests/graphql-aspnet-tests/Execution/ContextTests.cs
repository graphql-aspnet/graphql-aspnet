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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ContextTests
    {
        [Test]
        public void GraphFieldExecutionContext_PropertyCheck()
        {
            var parentContext = new Mock<IGraphQLMiddlewareExecutionContext>();

            var operationRequest = new Mock<IQueryOperationRequest>();
            var variableData = new Mock<IResolvedVariableCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<IGraphEventLogger>();
            var metrics = new Mock<IQueryExecutionMetrics>();
            var securityContext = new Mock<IUserSecurityContext>();

            parentContext.Setup(x => x.OperationRequest).Returns(operationRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            parentContext.Setup(x => x.SecurityContext).Returns(securityContext.Object);
            parentContext.Setup(x => x.Metrics).Returns(metrics.Object);
            parentContext.Setup(x => x.Logger).Returns(logger.Object);
            parentContext.Setup(x => x.Session).Returns(new QuerySession());

            var fieldRequest = new Mock<IGraphFieldRequest>();
            var sourceFieldCollection = new FieldSourceCollection();

            var context = new GraphFieldExecutionContext(
                parentContext.Object,
                fieldRequest.Object,
                variableData.Object,
                sourceFieldCollection);

            Assert.AreEqual(operationRequest.Object, context.OperationRequest);
            Assert.AreEqual(variableData.Object, context.VariableData);
            Assert.AreEqual(sourceFieldCollection, context.DefaultFieldSources);
            Assert.AreEqual(fieldRequest.Object, context.Request);
            Assert.IsEmpty(context.ResolvedSourceItems);
            Assert.AreEqual(serviceProvider.Object, context.ServiceProvider);
            Assert.AreEqual(logger.Object, context.Logger);
            Assert.AreEqual(metrics.Object, context.Metrics);
            Assert.AreEqual(securityContext.Object, context.SecurityContext);
        }

        [Test]
        public void GraphQueryExecutionContext_PropertyCheck()
        {
            var parentContext = new Mock<IGraphQLMiddlewareExecutionContext>();

            var operationRequest = new Mock<IQueryOperationRequest>();
            var variableData = new Mock<IResolvedVariableCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<IGraphEventLogger>();
            var metrics = new Mock<IQueryExecutionMetrics>();
            var securityContext = new Mock<IUserSecurityContext>();

            var fieldRequest = new Mock<IGraphFieldRequest>();
            var sourceFieldCollection = new FieldSourceCollection();

            var session = new QuerySession();
            var items = new MetaDataCollection();

            var context = new QueryExecutionContext(
                operationRequest.Object,
                serviceProvider.Object,
                session,
                items,
                securityContext.Object,
                metrics.Object,
                logger.Object);

            Assert.AreEqual(operationRequest.Object, context.OperationRequest);
            Assert.AreEqual(sourceFieldCollection, context.DefaultFieldSources);
            Assert.AreEqual(serviceProvider.Object, context.ServiceProvider);
            Assert.AreEqual(logger.Object, context.Logger);
            Assert.AreEqual(metrics.Object, context.Metrics);
            Assert.AreEqual(securityContext.Object, context.SecurityContext);
            Assert.AreEqual(session, context.Session);
            Assert.AreEqual(items, context.Items);
            Assert.IsNotNull(context.DefaultFieldSources);
            Assert.IsNotNull(context.FieldResults);
            Assert.IsNull(context.Result);
        }

        [Test]
        public void GraphDirectiveExecutionContext_PropertyCheck()
        {
            var parentContext = new Mock<IGraphQLMiddlewareExecutionContext>();

            var operationRequest = new Mock<IQueryOperationRequest>();
            var variableData = new Mock<IResolvedVariableCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<IGraphEventLogger>();
            var metrics = new Mock<IQueryExecutionMetrics>();
            var securityContext = new Mock<IUserSecurityContext>();
            var schema = new GraphSchema();

            parentContext.Setup(x => x.OperationRequest).Returns(operationRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            parentContext.Setup(x => x.SecurityContext).Returns(securityContext.Object);
            parentContext.Setup(x => x.Metrics).Returns(metrics.Object);
            parentContext.Setup(x => x.Logger).Returns(logger.Object);
            parentContext.Setup(x => x.Session).Returns(new QuerySession());

            var args = new Mock<IExecutionArgumentCollection>();
            var directiveRequest = new Mock<IGraphDirectiveRequest>();
            var sourceFieldCollection = new FieldSourceCollection();

            var context = new DirectiveResolutionContext(
                schema,
                parentContext.Object,
                directiveRequest.Object,
                args.Object);

            Assert.AreEqual(operationRequest.Object, context.OperationRequest);
            Assert.AreEqual(directiveRequest.Object, context.Request);
            Assert.AreEqual(serviceProvider.Object, context.ServiceProvider);
            Assert.AreEqual(logger.Object, context.Logger);
            Assert.AreEqual(metrics.Object, context.Metrics);
            Assert.AreEqual(securityContext.Object, context.SecurityContext);
            Assert.AreEqual(schema, context.Schema);
        }
    }
}