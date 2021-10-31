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
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Middleware.FieldAuthorization;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.PipelineContextBuilders;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

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
        private readonly ClaimsPrincipal _userAccount;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServer{TSchema}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="userAccount">The user account.</param>
        public TestServer(
            IServiceProvider serviceProvider,
            ClaimsPrincipal userAccount)
        {
            this.ServiceProvider = serviceProvider;
            _userAccount = userAccount;
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
            httpContext.User = _userAccount;
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
        /// Renders a completed query document in the same manner that the graphql server would as part of fulfilling a request.
        /// </summary>
        /// <param name="queryText">The query text to generate a document for.</param>
        /// <returns>IGraphQueryDocument.</returns>
        public IGraphQueryDocument CreateDocument(string queryText)
        {
            var generator = this.ServiceProvider.GetService<IGraphQueryDocumentGenerator<TSchema>>();
            return generator.CreateDocument(this.CreateSyntaxTree(queryText));
        }

        /// <summary>
        /// Renders a qualified query plan in the same manner that the graphql server would as part of fulfilling a request.
        /// </summary>
        /// <param name="queryText">The query text to generate a plan for.</param>
        /// <returns>Task&lt;IGraphQueryPlan&gt;.</returns>
        public Task<IGraphQueryPlan> CreateQueryPlan(string queryText)
        {
            var planGenerator = this.ServiceProvider.GetService<IGraphQueryPlanGenerator<TSchema>>();
            return planGenerator.CreatePlan(this.CreateSyntaxTree(queryText));
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
        /// Creates a mocked context for the execution of a single field of data against the given concrete type and field name. This
        /// context can be submitted against the field execution pipeline to generate a result.
        /// </summary>
        /// <typeparam name="TType">The concrete type to create the request against.
        /// Either a graph type, directive or controller.</typeparam>
        /// <param name="fieldName">Name of the field, on the type, as it exists in the schema.</param>
        /// <param name="sourceData">The source data to use as the input to the field. This can be changed, but must be supplied. A
        /// generic <see cref="object"/> will be used if not supplied.</param>
        /// <returns>IMockFieldRequest.</returns>
        public FieldContextBuilder CreateFieldContextBuilder<TType>(string fieldName, object sourceData = null)
        {
            var template = TemplateHelper.CreateFieldTemplate<TType>(fieldName);
            var fieldMaker = new GraphFieldMaker(this.Schema);
            var fieldResult = fieldMaker.CreateField(template);

            var builder = new FieldContextBuilder(
                this.ServiceProvider,
                _userAccount,
                fieldResult.Field,
                this.Schema,
                template as IGraphMethod);

            if (sourceData == null)
                builder.AddSourceData(new object());
            else
                builder.AddSourceData(sourceData);

            return builder;
        }

        /// <summary>
        /// Creates a new mocked query context to which standard parameters (query text, variables etc.) can be configured
        /// and then submitted to the top level query execution pipeline.
        /// </summary>
        /// <returns>MockOperationRequest.</returns>
        public QueryContextBuilder CreateQueryContextBuilder()
        {
            return new QueryContextBuilder(this.ServiceProvider, _userAccount);
        }

        /// <summary>
        /// Executes the configured builder through the engine and returns a reference to the completed
        /// operation result to be inspected. This method executes the parsing and execution phases only.
        /// </summary>
        /// <param name="builder">The builder from which to generate the required query cotnext.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<IGraphOperationResult> ExecuteQuery(QueryContextBuilder builder)
        {
            var context = builder.Build();
            await this.ExecuteQuery(context).ConfigureAwait(false);
            return context.Result;
        }

        /// <summary>
        /// Executes the provided query context through the engine. This method executes the parsing and execution phases only.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task ExecuteQuery(GraphQueryExecutionContext context)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphQueryExecutionContext>>();

            await pipeline.InvokeAsync(context, default).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the field pipeline for the given request, returning the raw, unaltered response.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public async Task ExecuteField(GraphFieldExecutionContext context)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphFieldExecutionContext>>();
            await pipeline.InvokeAsync(context, default).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation request through the engine and generates a JSON string output.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> RenderResult(QueryContextBuilder builder)
        {
            var result = await this.ExecuteQuery(builder).ConfigureAwait(false);
            return await this.RenderResult(result).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation request through the engine and generates a JSON string output.
        /// </summary>
        /// <param name="context">The query context to render out to a json string.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> RenderResult(GraphQueryExecutionContext context)
        {
            await this.ExecuteQuery(context).ConfigureAwait(false);
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
        /// <returns>Task.</returns>
        public async Task ExecuteFieldAuthorization(GraphFieldAuthorizationContext context)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphFieldAuthorizationContext>>();
            await pipeline.InvokeAsync(context, default).ConfigureAwait(false);
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
        /// Gets the claims principal of the user generated with the test server.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User => _userAccount;
    }
}