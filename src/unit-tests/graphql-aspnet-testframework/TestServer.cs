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
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.PipelineContextBuilders;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;

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
        public virtual IGraphQLHttpProcessor<TSchema> CreateHttpQueryProcessor()
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
        public virtual HttpContext CreateHttpContext(GraphQueryData requestData = null)
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
        /// Renders a completed query document in the same manner that the graphql server
        /// would as part of fulfilling a request. This method DOES NOT validate the document.
        /// </summary>
        /// <param name="queryText">The query text to generate a document for.</param>
        /// <returns>IGraphQueryDocument.</returns>
        public virtual IQueryDocument CreateDocument(string queryText)
        {
            var text = ReadOnlySpan<char>.Empty;
            if (queryText != null)
                text = queryText.AsSpan();

            var generator = this.ServiceProvider.GetService<IQueryDocumentGenerator<TSchema>>();

            var document = generator.CreateDocument(text);
            return document;
        }

        /// <summary>
        /// Renders a qualified query plan in the same manner that the graphql server would as part of fulfilling a request.
        /// </summary>
        /// <param name="queryText">The query text to generate a plan for.</param>
        /// <param name="operationName">Name of the operation in the query text to formalize
        /// into the plan.</param>
        /// <returns>Task&lt;IGraphQueryPlan&gt;.</returns>
        public virtual Task<IQueryExecutionPlan> CreateQueryPlan(string queryText, string operationName = null)
        {
            var documentGenerator = this.ServiceProvider.GetService<IQueryDocumentGenerator<TSchema>>();
            var planGenerator = this.ServiceProvider.GetService<IQueryExecutionPlanGenerator<TSchema>>();
            var document = this.CreateDocument(queryText);

            documentGenerator.ValidateDocument(document);

            IOperationDocumentPart docPart = null;
            if (document.Operations.Count == 1)
                docPart = document.Operations[0];
            else
                docPart = document.Operations.RetrieveOperation(operationName);

            return planGenerator.CreatePlanAsync(docPart);
        }

        /// <summary>
        /// Attempts to create a qualified graph type from the provided concrete type and kind reference from the
        /// schema served by the test server. Returns a result containing the graph type and any known dependencies.
        /// </summary>
        /// <param name="concreteType">The concrete type to convert.</param>
        /// <param name="kind">The kind of graph type to make, if any.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        public virtual GraphTypeCreationResult CreateGraphType(Type concreteType, TypeKind kind)
        {
            var factory = new GraphTypeMakerFactory(this.Schema);

            var maker = factory.CreateTypeMaker(concreteType, kind);
            var template = factory.MakeTemplate(concreteType);
            return maker.CreateGraphType(template);
        }

        /// <summary>
        /// (DEPRECATED, DO NOT USE).
        /// </summary>
        /// <typeparam name="TType">The concrete type representing the graph type in the schema.</typeparam>
        /// <param name="fieldName">Name of the field, on the type, as it exists in the schema.</param>
        /// <param name="sourceData">The source data to use as the input to the field. This can be changed, but must be supplied. A
        /// generic <see cref="object" /> will be used if not supplied.</param>
        /// <param name="typeKind">The type kind to resolve the field as (only necessary for input object types).</param>
        /// <returns>FieldContextBuilder.</returns>
        [Obsolete("Use " + nameof(CreateFieldContextBuilder) + " Instead")]
        public virtual FieldContextBuilder CreateGraphTypeFieldContextBuilder<TType>(string fieldName, object sourceData, TypeKind typeKind)
        {
            return CreateFieldContextBuilder<TType>(fieldName, sourceData);
        }

        /// <summary>
        /// (DEPRECATED, DO NOT USE).
        /// </summary>
        /// <typeparam name="TController">The type of the controller that owns the
        /// action.</typeparam>
        /// <param name="actionName">Name of the action/field in the controller, as it exists in the schema.</param>
        /// <returns>FieldContextBuilder.</returns>
        [Obsolete("Use " + nameof(CreateFieldContextBuilder) + " Instead")]
        public virtual FieldContextBuilder CreateActionMethodFieldContextBuilder<TController>(string actionName)
        {
            return CreateFieldContextBuilder<TController>(actionName);
        }

        /// <summary>
        /// Creates a builder that will generate a field execution context for an action on a target controller. This
        /// context can be submitted against the field execution pipeline to generate a result.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity that owns <paramref name="fieldOrActionName" />. This entity must be a
        /// controller or an entity representing an OBJECT graph type that is registered to the schema.</typeparam>
        /// <param name="fieldOrActionName">Name of the field as it appears in the graph or the name of the action, method or property
        /// as it appears on the <typeparamref name="TEntity"/>. This parameter is case sensitive.</param>
        /// <param name="sourceData">(optional) A source data object to supply to the builder.</param>
        /// <returns>A builder that can be invoked against a controller to resolve a field.</returns>
        public virtual FieldContextBuilder CreateFieldContextBuilder<TEntity>(string fieldOrActionName, object sourceData = null)
        {
            return this.CreateFieldContextBuilder(typeof(TEntity), fieldOrActionName, sourceData);
        }

        /// <summary>
        /// Attempts to search the schema for a field with the given internal name. If more than one field
        /// with the same name is found, the first instance is used. A context builder is then created
        /// for the found field.
        /// </summary>
        /// <param name="internalName">The internal name assigned to the fild.</param>
        /// <param name="sourceData">The source data.</param>
        /// <returns>A builder that can be invoked against a controller to resolve a field.</returns>
        public virtual FieldContextBuilder CreateFieldContextBuilder(string internalName, object sourceData = null)
        {
            IGraphField field = null;
            foreach (var f in this.Schema.AllSchemaItems())
            {
                if (f is IGraphField gf && gf.InternalName == internalName)
                {
                    field = gf;
                    break;
                }
            }

            if (field == null)
            {
                throw new InvalidOperationException($"A field with the internal name of '{internalName}' was not found. " +
                    $"Internal names for fields are case sensitive.");
            }

            return this.CreateFieldContextBuilder(field, sourceData);
        }

        /// <summary>
        /// Creates a builder targeting the field owned the the combination entity and field, method or property name.
        /// </summary>
        /// <param name="entityType">Type of the entity that owns the <paramref name="fieldOrActionName"/>. This entity must be a
        /// controller or an entity representing an OBJECT graph type that is registered to the schema</param>
        /// <param name="fieldOrActionName">Name of the field as it appears in the graph or the name of the action, method or property
        /// as it appears on the <paramref name="entityType" />. This parameter is case sensitive.</param>
        /// <param name="sourceData">(optional) A source data object to supply to the builder.</param>
        /// <returns>A builder that can be invoked against a controller to resolve a field.</returns>
        public virtual FieldContextBuilder CreateFieldContextBuilder(Type entityType, string fieldOrActionName, object sourceData = null)
        {
            Validation.ThrowIfNull(entityType, nameof(entityType));

            IGraphField field = null;
            fieldOrActionName = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldOrActionName, nameof(fieldOrActionName));

            if (Validation.IsCastable<GraphController>(entityType))
            {
                var fieldTemplate = GraphQLTemplateHelper.CreateFieldTemplate(entityType, fieldOrActionName);
                var fieldMaker = new GraphFieldMaker(this.Schema, new GraphArgumentMaker(this.Schema));
                field = fieldMaker.CreateField(fieldTemplate)?.Field;
            }
            else
            {
                var graphType = this.Schema.KnownTypes.FindGraphType(entityType, TypeKind.OBJECT) as IObjectGraphType;
                if (graphType == null)
                {
                    throw new InvalidOperationException($"Unknown or unregistered OBJECT graph for type {entityType.FriendlyName()}. This method " +
                        $"can only create a context builder for OBJECT graph types.");
                }

                var typedGraphType = graphType as ITypedSchemaItem;
                if (typedGraphType == null)
                {
                    throw new InvalidOperationException($"The target graph type '{graphType.Name}' is not a strongly typed graph type and cannot be invoked via this builder.");
                }

                var container = graphType as IGraphFieldContainer;
                if (container == null)
                {
                    throw new InvalidOperationException($"The target graph type '{graphType.Name}' is not a field container. No field context builder can be created.");
                }

                // find the field on the graph type
                field = graphType.Fields.FindField(fieldOrActionName);
                if (field == null)
                {
                    // fallback, try and find by method/property name
                    foreach (var item in graphType.Fields)
                    {
                        if (item.Resolver.MetaData.DeclaredName == fieldOrActionName)
                        {
                            field = item;
                            break;
                        }
                    }
                }
            }

            if (field == null)
            {
                throw new InvalidOperationException($"The entity '{entityType.FriendlyName()}' does not contain a field, action, method or property named '{fieldOrActionName}'. " +
                    $"Field names are case sensitive.");
            }

            return this.CreateFieldContextBuilder(field, sourceData);
        }

        /// <summary>
        /// Creates a builder targeting the supplied field.
        /// </summary>
        /// <param name="field">The field to create a context builder for.</param>
        /// <param name="sourceData">(optional) A source data object to supply to the builder.</param>
        /// <returns>A builder that can be invoked against a controller to resolve a field.</returns>
        public FieldContextBuilder CreateFieldContextBuilder(IGraphField field, object sourceData = null)
        {
            var metaData = field.Resolver.MetaData;
            var builder = new FieldContextBuilder(
                this.ServiceProvider,
                _userSecurityContext,
                field,
                this.Schema,
                metaData);

            builder.AddSourceData(sourceData);
            return builder;
        }

        /// <summary>
        /// /// (DEPRECATED, DO NOT USE).
        /// </summary>
        /// <typeparam name="TDirective">The type of the directive to invoke.</typeparam>
        /// <param name="location">The target location from where the directive is being called.</param>
        /// <param name="directiveTarget">The target object of the invocation.</param>
        /// <param name="phase">The phase of invocation.</param>
        /// <param name="origin">The origin in a source document, if any.</param>
        /// <param name="arguments">The arguments to pass to the directive, if any.</param>
        /// <returns>GraphDirectiveExecutionContext.</returns>
        [Obsolete("Use " + nameof(CreateDirectiveContextBuilder) + " Instead")]
        public virtual GraphDirectiveExecutionContext CreateDirectiveExecutionContext<TDirective>(
            DirectiveLocation location,
            object directiveTarget,
            DirectiveInvocationPhase phase = DirectiveInvocationPhase.SchemaGeneration,
            SourceOrigin origin = default,
            object[] arguments = null)
            where TDirective : class
        {
            var builder = this.CreateDirectiveContextBuilder(
                typeof(TDirective),
                location,
                directiveTarget,
                phase,
                origin,
                arguments);

            return builder.CreateExecutionContext();
        }

        /// <summary>
        /// Creates a builder targeting a specific directive. This builder can be used to create execution contexts (targeting middleware components)
        /// and resolution contexts (targeting specific directive resolvers) in order to perform different types of tests.
        /// </summary>
        /// <typeparam name="TDirectiveType">The type of the <see cref="GraphDirective"/> to build for.</typeparam>
        /// <param name="directiveLocation">The directive location, of those accepted by the directive.</param>
        /// <param name="directiveTarget">The directive target. Typically an <see cref="IDocumentPart" /> for execution directives
        /// or an <see cref="ISchemaItem" /> for type system directives.</param>
        /// <param name="phase">The phase of execution. This value will be passed to the middleware item or resolver.</param>
        /// <param name="origin">The origin location within a query document. This value will be passed to the middleware item
        /// or resolver.</param>
        /// <param name="arguments">The set of input argument values accepted by the directive. These arguments are expected
        /// to be supplied in order of definition within the schema and castable to the appropriate data types. This builder
        /// will NOT attempt any validation or mangling on these values.</param>
        /// <returns>DirectiveContextBuilder.</returns>
        public virtual DirectiveContextBuilder CreateDirectiveContextBuilder<TDirectiveType>(
            DirectiveLocation directiveLocation,
            object directiveTarget = null,
            DirectiveInvocationPhase phase = DirectiveInvocationPhase.QueryDocumentExecution,
            SourceOrigin origin = default,
            object[] arguments = null)
        {
            var builder = this.CreateDirectiveContextBuilder(
                typeof(TDirectiveType),
                directiveLocation,
                directiveTarget,
                phase,
                origin,
                arguments);

            return builder;
        }

        /// <summary>
        /// Creates a builder targeting a specific directive. This builder can be used to create execution contexts (targeting middleware components)
        /// and resolution contexts (targeting specific directive resolvers) in order to perform different types of tests.
        /// </summary>
        /// <remarks>
        /// If the target <paramref name="directiveType"/> is not a valid directive or not registered to the schema a context builder
        /// will still be created with null values to test various scenarios.
        /// </remarks>
        /// <param name="directiveType">The type of the directive that is registered to teh schema.</param>
        /// <param name="directiveLocation">The directive location, of those accepted by the directive.</param>
        /// <param name="directiveTarget">The directive target. Typically an <see cref="IDocumentPart" /> for execution directives
        /// or an <see cref="ISchemaItem" /> for type system directives.</param>
        /// <param name="phase">The phase of execution. This value will be passed to the middleware item or resolver.</param>
        /// <param name="origin">The origin location within a query document. This value will be passed to the middleware item
        /// or resolver.</param>
        /// <param name="arguments">The set of input argument values accepted by the directive. These arguments are expected
        /// to be supplied in order of definition within the schema and castable to the appropriate data types. This builder
        /// will NOT attempt any validation or mangling on these values.</param>
        /// <returns>DirectiveContextBuilder.</returns>
        public virtual DirectiveContextBuilder CreateDirectiveContextBuilder(
            Type directiveType,
            DirectiveLocation directiveLocation,
            object directiveTarget = null,
            DirectiveInvocationPhase phase = DirectiveInvocationPhase.QueryDocumentExecution,
            SourceOrigin origin = default,
            object[] arguments = null)
        {
            var directiveInstance = this.Schema.KnownTypes.FindGraphType(directiveType, TypeKind.DIRECTIVE) as IDirective;

            IGraphFieldResolverMetaData metadata = null;
            directiveInstance?.Resolver.MetaData.TryGetValue(directiveLocation, out metadata);

            var builder = new DirectiveContextBuilder(
                this.ServiceProvider,
                this.SecurityContext,
                this.Schema,
                directiveInstance,
                directiveLocation,
                phase,
                metadata);

            if (directiveTarget != null)
                builder.AddTarget(directiveTarget);

            if (arguments != null && directiveInstance != null)
            {
                for (var i = 0; i < arguments.Length; i++)
                {
                    if (directiveInstance.Arguments.Count > i)
                    {
                        var argByPosition = directiveInstance.Arguments[i];
                        builder.AddInputArgument(argByPosition.Name, arguments[i]);
                    }
                }
            }

            return builder;
        }

        /// <summary>
        /// Creates a reference to the invokable method or property that represents the
        /// root resolver for the given <paramref name="fieldName" />. (i.e. the method
        /// or property of the object that can produce the core data value).
        /// </summary>
        /// <typeparam name="TObjectType">The type of the object to create a reference from.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphMethod.</returns>
        public virtual IGraphFieldResolverMetaData CreateResolverMetadata<TObjectType>(string fieldName)
        {
            return CreateResolverMetadata(typeof(TObjectType), fieldName);
        }

        /// <summary>
        /// Creates a reference to the invokable method or property that acts as a resolver for the given <paramref name="entityType"/>.
        /// The <paramref name="entityType"/> must represent a controller or OBJECT graph type.
        /// </summary>
        /// <param name="entityType">Type of the entity that owns the field.</param>
        /// <param name="fieldOrActionName">Name of the field or method/property name to search for.</param>
        /// <returns>IGraphMethod.</returns>
        public virtual IGraphFieldResolverMetaData CreateResolverMetadata(Type entityType, string fieldOrActionName)
        {
            var builder = CreateFieldContextBuilder(entityType, fieldOrActionName);
            return builder?.ResolverMetaData;
        }

        /// <summary>
        /// Creates a new mocked query context to which standard parameters (query text, variables etc.) can be configured
        /// and then submitted to the top level query execution pipeline.
        /// </summary>
        /// <returns>QueryContextBuilder.</returns>
        public virtual QueryContextBuilder CreateQueryContextBuilder()
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
        public virtual async Task<IQueryExecutionResult> ExecuteQuery(QueryContextBuilder builder, CancellationToken cancelToken = default)
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
        public virtual async Task ExecuteQuery(QueryExecutionContext context, CancellationToken cancelToken = default)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, QueryExecutionContext>>();

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
        public virtual async Task ExecuteField(GraphFieldExecutionContext context, CancellationToken cancelToken = default)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphFieldExecutionContext>>();

            if (context != null)
                context.CancellationToken = cancelToken;
            await pipeline.InvokeAsync(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the field authorization pipeline against the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        public virtual async Task ExecuteFieldAuthorization(SchemaItemSecurityChallengeContext context, CancellationToken cancelToken = default)
        {
            var pipeline = this.ServiceProvider.GetService<ISchemaPipeline<TSchema, SchemaItemSecurityChallengeContext>>();
            await pipeline.InvokeAsync(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation request through the engine and generates a JSON string output.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public virtual async Task<string> RenderResult(QueryContextBuilder builder, CancellationToken cancelToken = default)
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
        public virtual async Task<string> RenderResult(QueryExecutionContext context, CancellationToken cancelToken = default)
        {
            await this.ExecuteQuery(context, cancelToken).ConfigureAwait(false);
            return await this.RenderResult(context.Result).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the provided operation result through the response writer and generates a JSON based string output.
        /// </summary>
        /// <param name="result">The result of a previously completed operation.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        protected virtual async Task<string> RenderResult(IQueryExecutionResult result)
        {
            var options = new ResponseWriterOptions()
            {
                ExposeExceptions = this.Schema.Configuration.ResponseOptions.ExposeExceptions,
                ExposeMetrics = this.Schema.Configuration.ResponseOptions.ExposeMetrics,
            };

            var writer = this.ServiceProvider.GetService<IQueryResponseWriter<TSchema>>();
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
        /// Creates a factory object that can generate templates and graph types.
        /// </summary>
        /// <returns>DefaultGraphQLTypeMakerFactory.</returns>
        public GraphTypeMakerFactory CreateMakerFactory()
        {
            return new GraphTypeMakerFactory(this.Schema);
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