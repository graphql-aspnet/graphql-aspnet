﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.PipelineContextBuilders
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Security;
    using Moq;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;

    /// <summary>
    /// A builder used to create inlined mocked replacements
    /// of various contained data to setup a test scenario targeting a single field resolution.
    /// </summary>
    public class FieldContextBuilder
    {
        private readonly IGraphField _graphField;
        private readonly ISchema _schema;
        private readonly Mock<IGraphFieldRequest> _mockRequest;
        private readonly Mock<IGraphFieldInvocationContext> _mockInvocationContext;
        private readonly Mock<IFieldDocumentPart> _mockFieldDocumentPart;
        private readonly IGraphMessageCollection _messageCollection;
        private readonly IInputArgumentCollection _arguments;

        private IUserSecurityContext _securityContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldContextBuilder" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="userSecurityContext">The user security context.</param>
        /// <param name="graphField">The graph field.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="metaData">The metadata describing the method/functon to be invoked by a resolver.</param>
        public FieldContextBuilder(
            IServiceProvider serviceProvider,
            IUserSecurityContext userSecurityContext,
            IGraphField graphField,
            ISchema schema,
            IGraphFieldResolverMetaData metaData)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _graphField = Validation.ThrowIfNullOrReturn(graphField, nameof(graphField));
            _securityContext = Validation.ThrowIfNullOrReturn(userSecurityContext, nameof(userSecurityContext));
            _messageCollection = new GraphMessageCollection();
            _arguments = InputArgumentCollectionFactory.Create();

            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));

            Type expectedInputType = null;

            if (!Validation.IsCastable<GraphDirective>(metaData.ParentObjectType)
                && !Validation.IsCastable<GraphController>(metaData.ParentObjectType))
            {
                if (graphField.Parent is IInterfaceGraphType iif)
                    expectedInputType = iif.ObjectType;
                else if (graphField.Parent is IObjectGraphType ogt)
                    expectedInputType = ogt.ObjectType;
            }

            _mockFieldDocumentPart = new Mock<IFieldDocumentPart>();
            _mockFieldDocumentPart.Setup(x => x.Field).Returns(_graphField);
            _mockFieldDocumentPart.Setup(x => x.Name).Returns(_graphField.Name);
            _mockFieldDocumentPart.Setup(x => x.Alias).Returns(_graphField.Name);

            _mockInvocationContext = new Mock<IGraphFieldInvocationContext>();
            _mockInvocationContext.Setup(x => x.ExpectedSourceType).Returns(expectedInputType);
            _mockInvocationContext.Setup(x => x.Field).Returns(_graphField);
            _mockInvocationContext.Setup(x => x.Arguments).Returns(_arguments);
            _mockInvocationContext.Setup(x => x.Name).Returns(_graphField.Name);
            _mockInvocationContext.Setup(x => x.ChildContexts).Returns(new FieldInvocationContextCollection());
            _mockInvocationContext.Setup(x => x.Origin).Returns(SourceOrigin.None);
            _mockInvocationContext.Setup(x => x.Schema).Returns(_schema);
            _mockInvocationContext.Setup(x => x.FieldDocumentPart).Returns(_mockFieldDocumentPart.Object);

            // fake the request for the field data
            // (normally generated by the primary query exeuction context)
            var id = Guid.NewGuid();
            _mockRequest = new Mock<IGraphFieldRequest>();
            _mockRequest.Setup(x => x.Id).Returns(id);
            _mockRequest.Setup(x => x.Origin).Returns(SourceOrigin.None);
            _mockRequest.Setup(x => x.Field).Returns(_graphField);
            _mockRequest.Setup(x => x.InvocationContext).Returns(_mockInvocationContext.Object);

            this.ResolverMetaData = new Mock<IGraphFieldResolverMetaData>();
            this.ResolverMetaData.Setup(x => x.ParentInternalFullName).Returns(metaData.ParentInternalFullName);
            this.ResolverMetaData.Setup(x => x.ParentInternalName).Returns(metaData.ParentInternalName);
            this.ResolverMetaData.Setup(x => x.ParentObjectType).Returns(metaData.ParentObjectType);
            this.ResolverMetaData.Setup(x => x.ExpectedReturnType).Returns(metaData.ExpectedReturnType);
            this.ResolverMetaData.Setup(x => x.Method).Returns(metaData.Method);
            this.ResolverMetaData.Setup(x => x.IsAsyncField).Returns(metaData.IsAsyncField);
            this.ResolverMetaData.Setup(x => x.InternalName).Returns(metaData.InternalName);
            this.ResolverMetaData.Setup(x => x.InternalFullName).Returns(metaData.InternalFullName);
            this.ResolverMetaData.Setup(x => x.InternalName).Returns(metaData.InternalName);
            this.ResolverMetaData.Setup(x => x.Parameters).Returns(metaData.Parameters);
        }

        /// <summary>
        /// Adds or replaces the source data supplied to the field request.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="path">An optional to mock where in the graph query the data originated.</param>
        /// <returns>MockFieldRequest.</returns>
        public FieldContextBuilder AddSourceData(object sourceData, SourcePath path = null)
        {
            path = path ?? SourcePath.None;
            var item = new FieldDataItem(_mockInvocationContext.Object, sourceData, path);
            var dataSource = new FieldDataItemContainer(sourceData, path, item);
            _mockRequest.Setup(x => x.Data).Returns(dataSource);
            return this;
        }

        /// <summary>
        /// Adds a custom origin to the field request instead of using <see cref="SourceOrigin.None"/>.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns>MockFieldExecutionContext.</returns>
        public FieldContextBuilder AddOrigin(SourceOrigin origin)
        {
            _mockRequest.Setup(x => x.Origin).Returns(origin);
            _mockInvocationContext.Setup(x => x.Origin).Returns(origin);
            return this;
        }

        /// <summary>
        /// Adds a value for a given input argument to this field. It is assumed the value will cast correctly.
        /// </summary>
        /// <param name="argumentName">Name of the input field, as declared in the schema, on the field being mocked.</param>
        /// <param name="value">A fully resolved value to use.</param>
        /// <returns>MockFieldRequest.</returns>
        public FieldContextBuilder AddInputArgument(string argumentName, object value)
        {
            var resolvedInputValue = new ResolvedInputArgumentValue(argumentName, value);
            var fieldArgument = _graphField.Arguments[argumentName];
            var inputArgument = new InputArgument(fieldArgument, resolvedInputValue, SourceOrigin.None);
            _arguments.Add(inputArgument);
            return this;
        }

        /// <summary>
        /// alters the security context to be different than that provided by the server that created this builder.
        /// </summary>
        /// <param name="securityContext">The security context.</param>
        /// <returns>MockFieldRequest.</returns>
        public FieldContextBuilder AddSecurityContext(IUserSecurityContext securityContext)
        {
            _securityContext = securityContext;
            return this;
        }

        private IGraphQLMiddlewareExecutionContext CreateFakeParentMiddlewareContext()
        {
            var queryRequest = new Mock<IQueryExecutionRequest>();
            var parentContext = new Mock<IGraphQLMiddlewareExecutionContext>();

            parentContext.Setup(x => x.QueryRequest).Returns(queryRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(this.ServiceProvider);
            parentContext.Setup(x => x.SecurityContext).Returns(_securityContext);
            parentContext.Setup(x => x.Metrics).Returns(null as IQueryExecutionMetrics);
            parentContext.Setup(x => x.Logger).Returns(null as IGraphEventLogger);
            parentContext.Setup(x => x.Messages).Returns(_messageCollection);
            parentContext.Setup(x => x.IsValid).Returns(_messageCollection.IsSucessful);
            parentContext.Setup(x => x.Session).Returns(new QuerySession());
            return parentContext.Object;
        }

        /// <summary>
        /// Creates an authorization context to validate the field request this builder is creating.
        /// </summary>
        /// <returns>GraphFieldAuthorizationContext.</returns>
        public SchemaItemSecurityChallengeContext CreateSecurityContext()
        {
            var parent = this.CreateFakeParentMiddlewareContext();

            var request = new SchemaItemSecurityRequest(this.FieldRequest);
            return new SchemaItemSecurityChallengeContext(
                parent,
                request);
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>GraphFieldExecutionContext.</returns>
        public GraphFieldExecutionContext CreateExecutionContext()
        {
            return new GraphFieldExecutionContext(
                this.CreateFakeParentMiddlewareContext(),
                this.FieldRequest,
                ResolvedVariableCollectionFactory.Create());
        }

        /// <summary>
        /// Creates the resolution context capable of being acted on directly by a resolver, as opposed to being
        /// processed through a pipeline.
        /// </summary>
        /// <returns>FieldResolutionContext.</returns>
        public FieldResolutionContext CreateResolutionContext()
        {
            var context = this.CreateExecutionContext();

            ExecutionArgumentGenerator.TryConvert(
                context.InvocationContext.Arguments,
                context.VariableData,
                context.Messages,
                out var executionArguments);

            executionArguments = executionArguments.ForContext(context);

            return new FieldResolutionContext(
                _schema,
                this.CreateFakeParentMiddlewareContext(),
                this.FieldRequest,
                executionArguments);
        }

        /// <summary>
        /// Gets a reference to the field request to be contained in the context.
        /// </summary>
        /// <value>The field request.</value>
        public IGraphFieldRequest FieldRequest => _mockRequest.Object;

        /// <summary>
        /// Gets or sets the service provider used in all created contexts.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Gets a mock reference to the root method that will be resonsible for resolving the context in case any direct
        /// invocation tests are needed. Otherwise, this property is not used in resolving a context put directly
        /// against the testserver.
        /// </summary>
        /// <value>The graph method.</value>
        public Mock<IGraphFieldResolverMetaData> ResolverMetaData { get; }
    }
}