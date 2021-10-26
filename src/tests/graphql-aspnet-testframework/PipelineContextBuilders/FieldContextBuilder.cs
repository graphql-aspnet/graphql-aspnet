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
    using System.Collections.Generic;
    using System.Security.Claims;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldAuthorization;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Variables;
    using Moq;

    /// <summary>
    /// A subclassed <see cref="GraphFieldExecutionContext"/> allowing for inline mocked replacements
    /// of various contained data to setup a test scenario.
    /// </summary>
    public class FieldContextBuilder
    {
        private readonly IGraphField _graphField;
        private readonly ISchema _schema;
        private readonly Mock<IGraphFieldRequest> _mockRequest;
        private readonly Mock<IGraphFieldInvocationContext> _mockInvocationContext;
        private readonly IGraphMessageCollection _messageCollection;
        private readonly InputArgumentCollection _arguments = new InputArgumentCollection();

        private ClaimsPrincipal _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldContextBuilder" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="user">The user.</param>
        /// <param name="graphField">The graph field.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="graphMethod">The metadata describing the method/functon to be invoked by a resolver.</param>
        public FieldContextBuilder(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user,
            IGraphField graphField,
            ISchema schema,
            IGraphMethod graphMethod)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _graphField = Validation.ThrowIfNullOrReturn(graphField, nameof(graphField));
            _user = Validation.ThrowIfNullOrReturn(user, nameof(user));
            _messageCollection = new GraphMessageCollection();

            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));

            Type expectedInputType = null;

            if (!Validation.IsCastable<GraphDirective>(graphMethod.Parent.ObjectType)
                && !Validation.IsCastable<GraphController>(graphMethod.Parent.ObjectType))
            {
                expectedInputType = graphMethod.Parent.ObjectType;
            }

            var metaData = new MetaDataCollection();
            _mockRequest = new Mock<IGraphFieldRequest>();
            _mockInvocationContext = new Mock<IGraphFieldInvocationContext>();

            // fake the request for the field data (normally generated by the primary query exeuction context)
            var id = Guid.NewGuid().ToString("N");
            _mockRequest.Setup(x => x.Id).Returns(id);
            _mockRequest.Setup(x => x.Origin).Returns(SourceOrigin.None);
            _mockRequest.Setup(x => x.Items).Returns(metaData);
            _mockRequest.Setup(x => x.Field).Returns(_graphField);
            _mockRequest.Setup(x => x.InvocationContext).Returns(_mockInvocationContext.Object);

            _mockInvocationContext.Setup(x => x.ExpectedSourceType).Returns(expectedInputType);
            _mockInvocationContext.Setup(x => x.Field).Returns(_graphField);
            _mockInvocationContext.Setup(x => x.Arguments).Returns(_arguments);
            _mockInvocationContext.Setup(x => x.Name).Returns(_graphField.Name);
            _mockInvocationContext.Setup(x => x.Directives).Returns(new List<IDirectiveInvocationContext>());
            _mockInvocationContext.Setup(x => x.ChildContexts).Returns(new FieldInvocationContextCollection());
            _mockInvocationContext.Setup(x => x.Origin).Returns(SourceOrigin.None);

            this.GraphMethod = new Mock<IGraphMethod>();
            this.GraphMethod.Setup(x => x.Parent).Returns(graphMethod.Parent);
            this.GraphMethod.Setup(x => x.ObjectType).Returns(graphMethod.ObjectType);
            this.GraphMethod.Setup(x => x.ExpectedReturnType).Returns(graphMethod.ExpectedReturnType);
            this.GraphMethod.Setup(x => x.Method).Returns(graphMethod.Method);
            this.GraphMethod.Setup(x => x.IsAsyncField).Returns(graphMethod.IsAsyncField);
            this.GraphMethod.Setup(x => x.Name).Returns(graphMethod.Name);
            this.GraphMethod.Setup(x => x.InternalFullName).Returns(graphMethod.InternalFullName);
            this.GraphMethod.Setup(x => x.InternalName).Returns(graphMethod.InternalName);
            this.GraphMethod.Setup(x => x.Route).Returns(graphMethod.Route);
            this.GraphMethod.Setup(x => x.Arguments).Returns(graphMethod.Arguments);
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
            var item = new GraphDataItem(_mockInvocationContext.Object, sourceData, path);
            var dataSource = new GraphFieldDataSource(sourceData, path, item);
            _mockRequest.Setup(x => x.DataSource).Returns(dataSource);
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
            var inputArgument = new InputArgument(fieldArgument, resolvedInputValue);
            _arguments.Add(inputArgument);
            return this;
        }

        /// <summary>
        /// alters the user account to be different than that provided by the server that created this builder.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>MockFieldRequest.</returns>
        public FieldContextBuilder AddUser(ClaimsPrincipal user)
        {
            _user = user;
            return this;
        }

        private IGraphMiddlewareContext CreateFakeParentMiddlewareContext()
        {
            var operationRequest = new Mock<IGraphOperationRequest>();
            var parentContext = new Mock<IGraphMiddlewareContext>();

            parentContext.Setup(x => x.OperationRequest).Returns(operationRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(this.ServiceProvider);
            parentContext.Setup(x => x.User).Returns(_user);
            parentContext.Setup(x => x.Metrics).Returns(null as IGraphQueryExecutionMetrics);
            parentContext.Setup(x => x.Logger).Returns(null as IGraphEventLogger);
            parentContext.Setup(x => x.Items).Returns(this.FieldRequest.Items ?? new MetaDataCollection());
            parentContext.Setup(x => x.Messages).Returns(_messageCollection);
            parentContext.Setup(x => x.IsValid).Returns(_messageCollection.IsSucessful);
            return parentContext.Object;
        }

        /// <summary>
        /// Creates an authorization context to validate the field request this builder is creating.
        /// </summary>
        /// <returns>GraphFieldAuthorizationContext.</returns>
        public GraphFieldAuthorizationContext CreateAuthorizationContext()
        {
            var parent = this.CreateFakeParentMiddlewareContext();

            var request = new GraphFieldAuthorizationRequest(this.FieldRequest);

            return new GraphFieldAuthorizationContext(
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
                new ResolvedVariableCollection());
        }

        /// <summary>
        /// Creates the resolution context capable of being acted on directly by a resolver, as opposed to being
        /// processed through a pipeline.
        /// </summary>
        /// <returns>FieldResolutionContext.</returns>
        public FieldResolutionContext CreateResolutionContext()
        {
            var context = this.CreateExecutionContext();
            var executionArguments = context
                .InvocationContext
                .Arguments
                .Merge(context.VariableData)
                .WithSourceData(context.Request.DataSource.Value);

            return new FieldResolutionContext(
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
        /// Gets a mock reference to the method that will be resonsible for resolving the context in case any direct
        /// invocation tests are needed. Otherwise, this property is not used in resolving a context put directly
        /// against the testserver.
        /// </summary>
        /// <value>The graph method.</value>
        public Mock<IGraphMethod> GraphMethod { get; }
    }
}