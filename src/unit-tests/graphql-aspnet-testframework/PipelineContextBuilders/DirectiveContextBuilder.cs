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
    using System.Threading;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using NSubstitute;

    /// <summary>
    /// A builder used to create inlined mocked replacements
    /// of various data to setup a test scenario targeting a single directive resolution.
    /// </summary>
    public class DirectiveContextBuilder
    {
        private readonly IDirective _directive;
        private readonly ISchema _schema;
        private readonly IGraphDirectiveRequest _mockRequest;
        private readonly IDirectiveInvocationContext _mockInvocationContext;
        private readonly IGraphMessageCollection _messageCollection;
        private readonly IInputArgumentCollection _arguments;
        private readonly IGraphFieldResolverMetaData _mockResolverMetadata;
        private DirectiveLocation _location;
        private IUserSecurityContext _securityContext;
        private object _directiveTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveContextBuilder" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="userSecurityContext">The user security context.</param>
        /// <param name="schema">The schema where the directive is declared.</param>
        /// <param name="directive">The directive to invoke.</param>
        /// <param name="directiveLocation">The location where this directive is targeting. This information is presented to the directive
        /// when it is invoked.</param>
        /// <param name="phase">The phase of execution where the directive is being processed.</param>
        /// <param name="resolverMetadata">The metadata describing the method/functon to be invoked by a resolver.</param>
        public DirectiveContextBuilder(
            IServiceProvider serviceProvider,
            IUserSecurityContext userSecurityContext,
            ISchema schema,
            IDirective directive,
            DirectiveLocation directiveLocation,
            DirectiveInvocationPhase phase,
            IGraphFieldResolverMetaData resolverMetadata)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _securityContext = Validation.ThrowIfNullOrReturn(userSecurityContext, nameof(userSecurityContext));
            _messageCollection = new GraphMessageCollection();
            _arguments = InputArgumentCollectionFactory.Create();
            _location = directiveLocation;
            _directive = directive;

            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));

            _mockInvocationContext = Substitute.For<IDirectiveInvocationContext>();
            _mockInvocationContext.Directive.Returns(_directive);
            _mockInvocationContext.Arguments.Returns(_arguments);
            _mockInvocationContext.Origin.Returns(SourceOrigin.None);
            _mockInvocationContext.Location.Returns((x) => _location);

            // fake the request for the directive
            // (normally generated by the primary query execution pipeline)
            var id = Guid.NewGuid();
            _mockRequest = Substitute.For<IGraphDirectiveRequest>();
            _mockRequest.Id.Returns(id);
            _mockRequest.Origin.Returns(SourceOrigin.None);
            _mockRequest.Directive.Returns(_directive);
            _mockRequest.DirectiveTarget.Returns((x) => _directiveTarget);
            _mockRequest.InvocationContext.Returns(_mockInvocationContext);
            _mockRequest.DirectivePhase.Returns(phase);

            // copy in the resolver to a controlled mock (vs. whatever implementation was passed)
            if (resolverMetadata != null)
            {
                _mockResolverMetadata = Substitute.For<IGraphFieldResolverMetaData>();
                _mockResolverMetadata.ParentInternalName.Returns(resolverMetadata.ParentInternalName);
                _mockResolverMetadata.ParentObjectType.Returns(resolverMetadata.ParentObjectType);
                _mockResolverMetadata.ExpectedReturnType.Returns(resolverMetadata.ExpectedReturnType);
                _mockResolverMetadata.Method.Returns(resolverMetadata.Method);
                _mockResolverMetadata.IsAsyncField.Returns(resolverMetadata.IsAsyncField);
                _mockResolverMetadata.InternalName.Returns(resolverMetadata.InternalName);
                _mockResolverMetadata.InternalName.Returns(resolverMetadata.InternalName);
                _mockResolverMetadata.Parameters.Returns(resolverMetadata.Parameters);
            }
        }

        /// <summary>
        /// Mimics a specific location in a document text. This value is passed to the request instead of
        /// using <see cref="SourceOrigin.None"/>. This information is handed to any context created by this builder.
        /// </summary>
        /// <param name="origin">An origin within a query document text.</param>
        /// <returns>MockFieldExecutionContext.</returns>
        public DirectiveContextBuilder AddOrigin(SourceOrigin origin)
        {
            _mockRequest.Origin.Returns(origin);
            _mockInvocationContext.Origin.Returns(origin);
            return this;
        }

        /// <summary>
        /// Simulate an argument being passed to the directive from a query document. It is assumed the value will cast correctly
        /// and will not cause an error.
        /// </summary>
        /// <param name="argumentName">Name of the argument on the directive, as declared in the schema.</param>
        /// <param name="value">A fully resolved value to use.</param>
        /// <returns>DirectiveContextBuilder.</returns>
        public DirectiveContextBuilder AddInputArgument(string argumentName, object value)
        {
            var resolvedInputValue = new ResolvedInputArgumentValue(argumentName, value);
            var arg = _directive.Arguments[argumentName];
            var inputArgument = new InputArgument(arg, resolvedInputValue, SourceOrigin.None);
            _arguments.Add(inputArgument);
            return this;
        }

        /// <summary>
        /// Alters the current security context to be different than that provided by the server that created this builder.
        /// </summary>
        /// <param name="securityContext">The new security context to use.</param>
        /// <returns>MockFieldRequest.</returns>
        public DirectiveContextBuilder AddSecurityContext(IUserSecurityContext securityContext)
        {
            _securityContext = securityContext;
            return this;
        }

        /// <summary>
        /// <para>
        /// Adds a specific target that is passed to a directive resolver during resolution.
        /// </para>
        /// <para>For execution directives this is an object that implements <see cref="IDocumentPart"/>.
        /// </para>
        /// <para>
        /// For type system directives this is an object that implements <see cref="ISchemaItem"/>.
        /// </para>
        /// </summary>
        /// <param name="target">The object being targeted by the directive invocation.</param>
        /// <returns>DirectiveContextBuilder.</returns>
        public DirectiveContextBuilder AddTarget(object target)
        {
            _directiveTarget = target;
            return this;
        }

        private IGraphQLMiddlewareExecutionContext CreateFakeParentMiddlewareContext()
        {
            var queryRequest = Substitute.For<IQueryExecutionRequest>();
            var parentContext = Substitute.For<IGraphQLMiddlewareExecutionContext>();

            parentContext.QueryRequest.Returns(queryRequest);
            parentContext.ServiceProvider.Returns(this.ServiceProvider);
            parentContext.SecurityContext.Returns(_securityContext);
            parentContext.Metrics.Returns(null as IQueryExecutionMetrics);
            parentContext.Logger.Returns(null as IGraphEventLogger);
            parentContext.Messages.Returns(_messageCollection);
            parentContext.IsValid.Returns(_messageCollection.IsSucessful);
            parentContext.Session.Returns(new QuerySession());
            return parentContext;
        }

        /// <summary>
        /// Creates an authorization context that can be used to test authorization middleware components.
        /// </summary>
        /// <returns>SchemaItemSecurityChallengeContext.</returns>
        public SchemaItemSecurityChallengeContext CreateSecurityContext()
        {
            var parent = this.CreateFakeParentMiddlewareContext();

            var request = new SchemaItemSecurityRequest(this.DirectiveRequest);
            return new SchemaItemSecurityChallengeContext(
                parent,
                request);
        }

        /// <summary>
        /// Creates a qualified execution context that can be passed to the directive execution pipeline
        /// to test middleware components.
        /// </summary>
        /// <returns>GraphDirectiveExecutionContext.</returns>
        public GraphDirectiveExecutionContext CreateExecutionContext()
        {
            return new GraphDirectiveExecutionContext(
                _schema,
                this.CreateFakeParentMiddlewareContext(),
                this.DirectiveRequest,
                ResolvedVariableCollectionFactory.Create(),
                _securityContext.DefaultUser);
        }

        /// <summary>
        /// Creates the resolution context that can be passed to a resolver for resolution.
        /// </summary>
        /// <returns>FieldResolutionContext.</returns>
        public DirectiveResolutionContext CreateResolutionContext()
        {
            var context = this.CreateExecutionContext();

            ExecutionArgumentGenerator.TryConvert(
                _arguments,
                context.VariableData,
                context.Messages,
                out var executionArguments);

            return new DirectiveResolutionContext(
                this.ServiceProvider,
                context.Session,
                _schema,
                context.QueryRequest,
                this.DirectiveRequest,
                executionArguments,
                context.Messages,
                context.Logger,
                context.User,
                CancellationToken.None);
        }

        /// <summary>
        /// Gets a reference to the directive request to be contained in the context.
        /// </summary>
        /// <value>The field request.</value>
        public IGraphDirectiveRequest DirectiveRequest => _mockRequest;

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
        /// <value>The metadata describing the resolver method to be invoked.</value>
        public IGraphFieldResolverMetaData ResolverMetaData => _mockResolverMetadata;
    }
}