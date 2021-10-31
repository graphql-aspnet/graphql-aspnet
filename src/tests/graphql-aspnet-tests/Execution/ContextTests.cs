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
    using System.Linq;
    using System.Security.Claims;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ContextTests
    {
        [Test]
        public void GraphFieldExecutionContext_PropertyCheck()
        {
            var parentContext = new Mock<IGraphExecutionContext>();

            var operationRequest = new Mock<IGraphOperationRequest>();
            var variableData = new Mock<IResolvedVariableCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<IGraphEventLogger>();
            var metrics = new Mock<IGraphQueryExecutionMetrics>();
            var user = new ClaimsPrincipal();
            var metadata = new MetaDataCollection();

            parentContext.Setup(x => x.OperationRequest).Returns(operationRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            parentContext.Setup(x => x.User).Returns(user);
            parentContext.Setup(x => x.Metrics).Returns(metrics.Object);
            parentContext.Setup(x => x.Logger).Returns(logger.Object);
            parentContext.Setup(x => x.Items).Returns(metadata);

            var fieldRequest = new Mock<IGraphFieldRequest>();
            var sourceFieldCollection = new DefaultFieldSourceCollection();

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
            Assert.AreEqual(user, context.User);
            Assert.AreEqual(metadata, context.Items);
        }

        [Test]
        public void GraphQueryExecutionContext_PropertyCheck()
        {
            var parentContext = new Mock<IGraphExecutionContext>();

            var operationRequest = new Mock<IGraphOperationRequest>();
            var variableData = new Mock<IResolvedVariableCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<IGraphEventLogger>();
            var metrics = new Mock<IGraphQueryExecutionMetrics>();
            var user = new ClaimsPrincipal();
            var metadata = new MetaDataCollection();

            var fieldRequest = new Mock<IGraphFieldRequest>();
            var sourceFieldCollection = new DefaultFieldSourceCollection();

            var context = new GraphQueryExecutionContext(
                operationRequest.Object,
                serviceProvider.Object,
                user,
                metrics.Object,
                logger.Object,
                metadata);

            Assert.AreEqual(operationRequest.Object, context.OperationRequest);
            Assert.AreEqual(sourceFieldCollection, context.DefaultFieldSources);
            Assert.AreEqual(serviceProvider.Object, context.ServiceProvider);
            Assert.AreEqual(logger.Object, context.Logger);
            Assert.AreEqual(metrics.Object, context.Metrics);
            Assert.AreEqual(user, context.User);
            Assert.AreEqual(metadata, context.Items);
            Assert.IsNotNull(context.DefaultFieldSources);
            Assert.IsNotNull(context.PostProcessingActions);
            Assert.IsNotNull(context.FieldResults);
            Assert.IsNull(context.Result);
        }

        [Test]
        public void GraphDirectiveExecutionContext_PropertyCheck()
        {
            var parentContext = new Mock<IGraphExecutionContext>();

            var operationRequest = new Mock<IGraphOperationRequest>();
            var variableData = new Mock<IResolvedVariableCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<IGraphEventLogger>();
            var metrics = new Mock<IGraphQueryExecutionMetrics>();
            var user = new ClaimsPrincipal();
            var metadata = new MetaDataCollection();

            parentContext.Setup(x => x.OperationRequest).Returns(operationRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            parentContext.Setup(x => x.User).Returns(user);
            parentContext.Setup(x => x.Metrics).Returns(metrics.Object);
            parentContext.Setup(x => x.Logger).Returns(logger.Object);
            parentContext.Setup(x => x.Items).Returns(metadata);

            var args = new Mock<IExecutionArgumentCollection>();
            var directiveRequest = new Mock<IGraphDirectiveRequest>();
            var sourceFieldCollection = new DefaultFieldSourceCollection();

            var context = new DirectiveResolutionContext(
                parentContext.Object,
                directiveRequest.Object,
                args.Object);

            Assert.AreEqual(operationRequest.Object, context.OperationRequest);
            Assert.AreEqual(directiveRequest.Object, context.Request);
            Assert.AreEqual(serviceProvider.Object, context.ServiceProvider);
            Assert.AreEqual(logger.Object, context.Logger);
            Assert.AreEqual(metrics.Object, context.Metrics);
            Assert.AreEqual(user, context.User);
            Assert.AreEqual(metadata, context.Items);
        }
    }
}