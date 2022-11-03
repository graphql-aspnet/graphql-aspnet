// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.PipelineContextBuilders;
    using GraphQL.AspNet.Variables;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// A mocked server instance built for a given schema and with a service provider (such as would exist at runtime)
    /// via ASP.NET. This test server instance allows for the creation of requests against it and the resolution
    /// of those requests at various levels in the server stack. For the sake of unit testing the authorization
    /// and user account permissions are fixed after it is built.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema.</typeparam>
    public class TestServer<TSchema>
         where TSchema : class, ISchema
    {
        private readonly IUserSecurityContext _userSecurityContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServer{TSchema}" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="userSecurityContext">The user security context.</param>
        public TestServer(
            IServiceProvider serviceProvider,
            IUserSecurityContext userSecurityContext)
        {
            this.ServiceProvider = serviceProvider;
            _userSecurityContext = userSecurityContext;
            this.Schema = serviceProvider.GetService<TSchema>();
        }

        /// <summary>
        /// Creates an instance of <see cref="DefaultGraphQLHttpProcessor{TSchema}"/> that can simulate a
        /// ASP.NET calling a method at runtime. The controller type must have been registered when the
        /// server was built or null will be returned.
        /// </summary>
        /// <returns>GraphQueryController&lt;TSchema&gt;.</returns>
        public IGraphQLHttpProcessor<TSchema> CreateHttpQueryProcessor()
        {
            var httpProcessor = this.ServiceProvider.GetService<IGraphQLHttpProcessor<TSchema>>();
            if (httpProcessor == null)
            {
                throw new InvalidOperationException($"Test Setup Error. Unable to instantiate query processor " +
                    $"through the interface '{typeof(IGraphQLHttpProcessor<TSchema>).FriendlyName()}'. Don't forget to add reference when building the test server.");
            }

            return httpProcessor;
        }

        /// <summary>
        /// Creates a mocked http context that can be passed through a query processor for testing with the given
        /// query data on the request.
        /// </summary>
        /// <param name="requestData">The request data.</param>
        /// <returns>HttpContext.</returns>
        public HttpContext CreateHttpContext(GraphQueryData requestData = null)
        {
            var requestStream = new MemoryStream();
            var responseStream = new MemoryStream();

            var httpContext = new DefaultHttpContext();
            httpContext.User = _userSecurityContext?.DefaultUser;
            httpContext.RequestServices = this.ServiceProvider;
            httpContext.Response.Body = responseStream;
            httpContext.Request.Method = HttpMethods.Post.ToUpper();
            httpContext.Request.Body = requestStream;

            if (requestData != null)
            {
                var writerOptions = new JsonWriterOptions()
                {
                    Indented = this.Schema.Configuration.ResponseOptions.IndentDocument,
                };

                var serializerSettings = new JsonSerializerOptions();
                serializerSettings.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

                using (var writer = new Utf8JsonWriter(requestStream, writerOptions))
                {
                    JsonSerializer.Serialize(writer, requestData, serializerSettings);
                    requestStream.Seek(0, SeekOrigin.Begin);
                }
            }

            return httpContext;
        }

        /// <summary>
        /// Renders a syntax tree in the same manner that the graphql server would as part of fulfilling a request.
        /// </summary>
        /// <param name="queryText">The query text to create a syntax tree for.</param>
        /// <returns>ISyntaxTree.</returns>
        public ISyntaxTree CreateSyntaxTree(string queryText)
        {
            var parser = this.ServiceProvider.GetService<IGraphQLDocumentParser>();
            return parser.ParseQueryDocument(queryText?.AsMemory() ?? ReadOnlyMemory<char>.Empty);
        }

        /// <summary>
        /// Renders a completed query document in the same manner that the graphql server
        /// would as part of fulfilling a request. This method DOES NOT validate the document.
        /// </summary>
        /// <param name="queryText">The query text to generate a document for.</param>
        /// <returns>IGraphQueryDocument.</returns>
        public IGraphQueryDocument CreateDocument(string queryText)
        {
            var generator = this.ServiceProvider.GetService<IGraphQueryDocumentGenerator<TSchema>>();
            var document = generator.CreateDocument(this.CreateSyntaxTree(queryText));
            return document;
        }

        /// <summary>
        /// Renders a qualified query plan in the same manner that the graphql server would as part of fulfilling a request.
        /// </summary>
        /// <param name="queryText">The query text to generate a plan for.</param>
        /// <param name="operationName">Name of the operation in the query text to formalize
        /// into the plan.</param>
        /// <returns>Task&lt;IGraphQueryPlan&gt;.</returns>
        public Task<IGraphQueryPlan> CreateQueryPlan(string queryText, string operationName = null)
        {
            var documentGenerator = this.ServiceProvider.GetService<IGraphQueryDocumentGenerator<TSchema>>();
            var planGenerator = this.ServiceProvider.GetService<IGraphQueryPlanGenerator<TSchema>>();
            var document = this.CreateDocument(queryText);

            documentGenerator.ValidateDocument(document);

            IOperationDocumentPart docPart = null;
            if (document.Operations.Count == 1)
                docPart = document.Operations[0];
            else
                docPart = document.Operations.RetrieveOperation(operationName);

            return planGenerator.CreatePlan(docPart);
        }

        /// <summary>
        /// Attempts to create a qualified graph type from the provided concrete type and kind reference from the
        /// schema served by the test server. Returns a result containing the graph type and any known dependencies.
        /// </summary>
        /// <param name="concreteType">The concrete type to convert.</param>
        /// <param name="kind">The kind of graph type to make, if any.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        public GraphTypeCreationResult CreateGraphType(Type concreteType, TypeKind kind)
        {
            var maker = GraphQLProviders.GraphTypeMakerProvider.CreateTypeMaker(this.Schema, kind);
            return maker.CreateGraphType(concreteType);
        }

        /// <summary>
        /// Creates a mocked context for the execution of an action on a target controller. This
        /// context can be submitted against the field execution pipeline to generate a result.
        /// </summary>
        /// <typeparam name="TController">The type of the controller that owns the
        /// action.</typeparam>
        /// <param name="actionName">Name of the action/field in the controller, as it exists in the schema.</param>
        /// <returns>FieldContextBuilder.</returns>
        public FieldContextBuilder CreateGraphTypeFieldContextBuilder<TController>(string actionName)
            where TController : GraphController
        {
            var template = TemplateHelper.CreateFieldTemplate<TController>(actionName);
            var fieldMaker = new GraphFieldMaker(this.Schema);
            var fieldResult = fieldMaker.CreateField(template);

            var builder = new FieldContextBuilder(
                this.ServiceProvider,
                _userSecurityContext,
                fieldResult.Field,
                this.Schema,
                template as IGraphMethod);

            builder.AddSourceData(new object());
            return builder;
        }

        /// <summary>
        /// Creates a fully resolved field context that can be processed by the test server.
        /// </summary>
        /// <typeparam name="TType">The concrete type representing the graph type in the schema.</typeparam>
        /// <param name="fieldName">Name of the field, on the type, as it exists in the schema.</param>
        /// <param name="sourceData">The source data to use as the input to the field. This can be changed, but must be supplied. A
        /// generic <see cref="object" /> will be used if not supplied.</param>
        /// <param name="arguments">The collection of arguments that need to be supplied
        /// to the field to properly resolve it.</param>
        /// <returns>FieldContextBuilder.</returns>
        public GraphFieldExecutionContext CreateFieldExecutionContext<TType>(
            string fieldName,
            object sourceData,
            InputArgumentCollection arguments = null)
        {
            IGraphType graphType = this.Schema.KnownTypes.FindGraphType(typeof(TType));

            if (graphType == null)
                Assert.Fail($"Unable to locate a registered graph type that matched the supplied source data (Type: {typeof(TType).FriendlyName()})");

            var typedGraphType = graphType as ITypedSchemaItem;
            if (typedGraphType == null)
                Assert.Fail($"The target graph type '{graphType.Name}' is not a strongly typed graph type and cannot be invoked via this builder.");

            var container = graphType as IGraphFieldContainer;
            if (container == null)
                Assert.Fail($"The target graph type '{graphType.Name}' is not a field container. No field context builder can be created.");

            var field = container.Fields.FindField(fieldName);
            if (field == null)
                Assert.Fail($"The target graph type '{graphType.Name}' does not contain a field named '{fieldName}'.");

            arguments = arguments ?? new InputArgumentCollection();
            var messages = new GraphMessageCollection();
            var metaData = new MetaDataCollection();

            var operationRequest = new Mock<IGraphOperationRequest>();
            var fieldInvocationContext = new Mock<IGraphFieldInvocationContext>();
            var parentContext = new Mock<IGraphExecutionContext>();
            var graphFieldRequest = new Mock<IGraphFieldRequest>();
            var fieldDocumentPart = new Mock<IFieldDocumentPart>();

            operationRequest.Setup(x => x.Items).Returns(metaData);

            parentContext.Setup(x => x.OperationRequest).Returns(operationRequest.Object);
            parentContext.Setup(x => x.ServiceProvider).Returns(this.ServiceProvider);
            parentContext.Setup(x => x.SecurityContext).Returns(this.SecurityContext);
            parentContext.Setup(x => x.Metrics).Returns(null as IGraphQueryExecutionMetrics);
            parentContext.Setup(x => x.Logger).Returns(null as IGraphEventLogger);
            parentContext.Setup(x => x.Messages).Returns(() => messages);
            parentContext.Setup(x => x.IsValid).Returns(() => messages.IsSucessful);
            parentContext.Setup(x => x.Session).Returns(new QuerySession());

            fieldDocumentPart.Setup(x => x.Name).Returns(field.Name);
            fieldDocumentPart.Setup(x => x.Alias).Returns(field.Name);
            fieldDocumentPart.Setup(x => x.Field).Returns(field);

            fieldInvocationContext.Setup(x => x.ExpectedSourceType).Returns(typeof(TType));
            fieldInvocationContext.Setup(x => x.Field).Returns(field);
            fieldInvocationContext.Setup(x => x.Arguments).Returns(arguments);
            fieldInvocationContext.Setup(x => x.Name).Returns(field.Name);
            fieldInvocationContext.Setup(x => x.ChildContexts).Returns(new FieldInvocationContextCollection());
            fieldInvocationContext.Setup(x => x.Origin).Returns(SourceOrigin.None);
            fieldInvocationContext.Setup(x => x.Schema).Returns(this.Schema);
            fieldInvocationContext.Setup(x => x.FieldDocumentPart).Returns(fieldDocumentPart.Object);

            var resolvedParentDataItem = new GraphDataItem(
                fieldInvocationContext.Object,
                sourceData,
                SourcePath.None);

            var sourceDataContainer = new GraphDataContainer(
                sourceData,
                SourcePath.None,
                resolvedParentDataItem);

            var id = Guid.NewGuid().ToString("N");
            graphFieldRequest.Setup(x => x.Id).Returns(id);
            graphFieldRequest.Setup(x => x.Origin).Returns(SourceOrigin.None);
            graphFieldRequest.Setup(x => x.Field).Returns(field);
            graphFieldRequest.Setup(x => x.InvocationContext).Returns(fieldInvocationContext.Object);
            graphFieldRequest.Setup(x => x.Data).Returns(() => sourceDataContainer);

            return new GraphFieldExecutionContext(
                parentContext.Object,
                graphFieldRequest.Object,
                new ResolvedVariableCollection());
        }

        /// <summary>
        /// Creates a mocked context for the execution of a single field of data against the given concrete type and field name. This
        /// context can be submitted against the field execution pipeline to generate a result.
        /// </summary>
        /// <typeparam name="TType">The concrete type representing the graph type in the schema.</typeparam>
        /// <param name="fieldName">Name of the field, on the type, as it exists in the schema.</param>
        /// <param name="sourceData">The source data to use as the input to the field. This can be changed, but must be supplied. A
        /// generic <see cref="object" /> will be used if not supplied.</param>
        /// <param name="typeKind">The type kind to resolve the field as (only necessary for input object types).</param>
        /// <returns>FieldContextBuilder.</returns>
        public FieldContextBuilder CreateGraphTypeFieldContextBuilder<TType>(string fieldName, object sourceData, TypeKind? typeKind = null)
        {
            IGraphType graphType = this.Schema.KnownTypes.FindGraphType(typeof(TType));

            if (graphType == null)
                Assert.Fail($"Unable to locate a registered graph type that matched the supplied source data (Type: {typeof(TType).FriendlyName()})");

            var typedGraphType = graphType as ITypedSchemaItem;
            if (typedGraphType == null)
                Assert.Fail($"The target graph type '{graphType.Name}' is not a strongly typed graph type and cannot be invoked via this builder.");

            var container = graphType as IGraphFieldContainer;
            if (container == null)
                Assert.Fail($"The target graph type '{graphType.Name}' is not a field container. No field context builder can be created.");

            var field = container.Fields.FindField(fieldName);
            if (field == null)
                Assert.Fail($"The target graph type '{graphType.Name}' does not contain a field named '{fieldName}'.");

            var graphMethod = this.CreateInvokableReference<TType>(fieldName, typeKind);

            var builder = new FieldContextBuilder(
                this.ServiceProvider,
                _userSecurityContext,
                field,
                this.Schema,
                graphMethod);

            builder.AddSourceData(sourceData);

            return builder;
        }

        /// <summary>
        /// Creates a reference to the invokable method or property that represents the
        /// root resolver for the given <paramref name="fieldName"/>. (i.e. the method
        /// or property of the object that can produce the core data value).
        /// </summary>
        /// <typeparam name="TObjectType">The type of the t object type.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="typeKind">The type kind to resolve the field as (only necessary for input object types).</param>
        /// <returns>IGraphMethod.</returns>
        public IGraphMethod CreateInvokableReference<TObjectType>(string fieldName, TypeKind? typeKind = null)
        {
            var template = TemplateHelper.CreateGraphTypeTemplate<TObjectType>(typeKind);
            var fieldContainer = template as IGraphTypeFieldTemplateContainer;
            if (fieldContainer == null)
            {
                Assert.Fail($"The provided type '{typeof(TObjectType).FriendlyName()}' is not " +
                    $"a field container, no invokable method references can be created from it.");
            }

            var fieldTemplate = fieldContainer.FieldTemplates
                .SingleOrDefault(x => string.Compare(x.Value.Name, fieldName, StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            if (fieldTemplate == null)
            {
                Assert.Fail($"The provided type '{typeof(TObjectType).FriendlyName()}' does not " +
                      $"contain a field named '{fieldName}'.");
            }

            var method = fieldTemplate as IGraphMethod;
            if (method == null)
            {
                Assert.Fail($"The field named '{fieldName}' on the provided type '{typeof(TObjectType).FriendlyName()}' " +
                      $"does not represent an invokable {typeof(IGraphMethod)}. Operation cannot proceed.");
            }

            return method;
        }

        /// <summary>
        /// Creates an execution context to invoke a directive execution pipeleine.
        /// </summary>
        /// <typeparam name="TDirective">The type of the directive to invoke.</typeparam>
        /// <param name="location">The target location of the invocation.</param>
        /// <param name="directiveTarget">The target object of hte invocation.</param>
        /// <param name="phase">The phase of invocation.</param>
        /// <param name="origin">The origin in a source document, if any.</param>
        /// <param name="arguments">The arguments to pass to the directive, if any.</param>
        /// <returns>GraphDirectiveExecutionContext.</returns>
        public GraphDirectiveExecutionContext CreateDirectiveExecutionContext<TDirective>(
            DirectiveLocation location,
            object directiveTarget,
            DirectiveInvocationPhase phase = DirectiveInvocationPhase.SchemaGeneration,
            SourceOrigin origin = null,
            object[] arguments = null)
            where TDirective : class
        {
            origin = origin ?? SourceOrigin.None;

            var server = new TestServerBuilder()
                .AddType<TDirective>()
                .Build();

            var targetDirective = server.Schema.KnownTypes.FindDirective(typeof(TDirective));

            var operationRequest = new Mock<IGraphOperationRequest>();
            var directiveRequest = new Mock<IGraphDirectiveRequest>();
            var invocationContext = new Mock<IDirectiveInvocationContext>();
            var argCollection = new InputArgumentCollection();

            directiveRequest.Setup(x => x.DirectivePhase).Returns(phase);
            directiveRequest.Setup(x => x.InvocationContext).Returns(invocationContext.Object);
            directiveRequest.Setup(x => x.DirectiveTarget).Returns(directiveTarget);

            invocationContext.Setup(x => x.Location).Returns(location);
            invocationContext.Setup(x => x.Arguments).Returns(argCollection);
            invocationContext.Setup(x => x.Origin).Returns(origin);
            invocationContext.Setup(x => x.Directive).Returns(targetDirective);

            if (targetDirective != null && targetDirective.Kind == TypeKind.DIRECTIVE
                && arguments != null)
            {
                for (var i = 0; i < targetDirective.Arguments.Count; i++)
                {
                    if (arguments.Length <= i)
                        break;

                    var directiveArg = targetDirective.Arguments[i];
                    var resolvedValue = arguments[i];

                    var argValue = new ResolvedInputArgumentValue(directiveArg.Name, resolvedValue);
                    var inputArg = new InputArgument(directiveArg, argValue);
                    argCollection.Add(inputArg);
                }
            }

            var context = new GraphDirectiveExecutionContext(
                server.Schema,
                directiveRequest.Object,
                operationRequest.Object,
                server.ServiceProvider,
                new QuerySession());

            return context;
        }

        /// <summary>
        /// Creates a new mocked query context to which standard parameters (query text, variables etc.) can be configured
        /// and then submitted to the top level query execution pipeline.
        /// </summary>
        /// <returns>MockOperationRequest.</returns>
        public QueryContextBuilder CreateQueryContextBuilder()
        {
            return new QueryContextBuilder(this.ServiceProvider, _userSecurityContext);
        }

        /// <summary>
        /// Executes the configured builder through the engine and returns a reference to the completed
        /// operation result to be inspected. This method executes the parsing and execution phases only.
        /// </summary>
        /// <param name="builder">The builder from which to generate the required query cotnext.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<IGraphOperationResult> ExecuteQuery(QueryContextBuilder builder, CancellationToken cancelToken = default)
        {
            var context = builder.Build();
            await this.ExecuteQuery(context, cancelToken).ConfigureAwait(false);
            return context.Result;
        }

        /// <summary>
        /// Executes the provided query context through the engine. This method executes the parsing and execution phases only.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task ExecuteQuery(GraphQueryExecutionContext context, CancellationToken cancelToken = default)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphQueryExecutionContext>>();

            if (context != null)
                context.CancellationToken = cancelToken;

            await pipeline.InvokeAsync(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the field pipeline for the given request, returning the raw, unaltered response.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public async Task ExecuteField(GraphFieldExecutionContext context, CancellationToken cancelToken = default)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphFieldExecutionContext>>();

            if (context != null)
                context.CancellationToken = cancelToken;
            await pipeline.InvokeAsync(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation request through the engine and generates a JSON string output.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> RenderResult(QueryContextBuilder builder, CancellationToken cancelToken = default)
        {
            var result = await this.ExecuteQuery(builder, cancelToken).ConfigureAwait(false);
            return await this.RenderResult(result).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation request through the engine and generates a JSON string output.
        /// </summary>
        /// <param name="context">The query context to render out to a json string.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> RenderResult(GraphQueryExecutionContext context, CancellationToken cancelToken = default)
        {
            await this.ExecuteQuery(context, cancelToken).ConfigureAwait(false);
            return await this.RenderResult(context.Result).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation result through the response writer and generates a JSON based string output.
        /// </summary>
        /// <param name="result">The result of a previously completed operation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        private async Task<string> RenderResult(IGraphOperationResult result)
        {
            var options = new GraphQLResponseOptions()
            {
                ExposeExceptions = this.Schema.Configuration.ResponseOptions.ExposeExceptions,
                ExposeMetrics = this.Schema.Configuration.ResponseOptions.ExposeMetrics,
            };

            var writer = this.ServiceProvider.GetService<IGraphResponseWriter<TSchema>>();
            using (var memStream = new MemoryStream())
            {
                await writer.WriteAsync(memStream, result, options).ConfigureAwait(false);
                memStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Executes the field authorization pipeline against the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        public async Task ExecuteFieldAuthorization(GraphSchemaItemSecurityChallengeContext context, CancellationToken cancelToken = default)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphSchemaItemSecurityChallengeContext>>();
            await pipeline.InvokeAsync(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a reference to the schema this test server is hosting.
        /// </summary>
        /// <value>The schema.</value>
        public TSchema Schema { get; }

        /// <summary>
        /// Gets a reference to the service provider that is acting for this test server.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the security context created to mock a user on this server.
        /// </summary>
        /// <value>The security context.</value>
        public IUserSecurityContext SecurityContext => _userSecurityContext;
    }
}