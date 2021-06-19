// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.QueryExecution
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A decorator for the query execution pipeline builder to configure default components.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper will add components for.</typeparam>
    public class QueryExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutionPipelineHelper{TSchema}" /> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public QueryExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> pipelineBuilder)
        {
            this.PipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public virtual QueryExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            this.AddValidateRequestMiddleware()
                .AddRecordQueryMetricsMiddleware();

            if (options?.CacheOptions != null && !options.CacheOptions.Disabled)
                this.AddQueryPlanCacheMiddleware();

            this.AddQueryDocumentParsingMiddleware()
                .AddQueryPlanCreationMiddleware()
                .AddQueryAssignOperationMiddleware();

            var authOption = options?.AuthorizationOptions?.Method ?? AuthorizationMethod.PerField;
            if (authOption == Security.AuthorizationMethod.PerRequest)
            {
                this.AddQueryOperationAuthorizationMiddleware();
            }

            return this
                .AddQueryPlanExecutionMiddleware()
                .AddResultCreationMiddleware();
        }

        /// <summary>
        /// Adds the middleware component to authorize the user on the request to every secured field prior
        /// to resolving any fields on the chosen operation.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryOperationAuthorizationMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<AuthorizeQueryOperationMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component to ensure the context is valid and can be executed by the pipeline
        /// before starting the process.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddValidateRequestMiddleware()
        {
            // add the validator component as an instance so as not to rely on DI
            this.PipelineBuilder.AddMiddleware(new ValidateQueryRequestMiddleware());
            return this;
        }

        /// <summary>
        /// Adds the middleware component to start/stop the master metrics recording process
        /// for the context.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddRecordQueryMetricsMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<RecordQueryMetricsMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component for retrieving a query plan from the query cache.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryPlanCacheMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<QueryPlanCacheMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component for parsing query text into an AST.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryDocumentParsingMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<ParseQueryDocumentMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component for generating a query plan from a parsed AST.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryPlanCreationMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<GenerateQueryPlanMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that will assign the correct operation, from the query plan, to the active context
        /// for the request.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryAssignOperationMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<AssignQueryOperationMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that will execute a query plan for a request.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryPlanExecutionMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<ExecuteQueryOperationMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that will generate a final reslt after the plan is executed.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddResultCreationMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<PackageQueryResultMiddleware>();
            return this;
        }

        /// <summary>
        /// Gets the pipeline builder being worked on in this helper.
        /// </summary>
        /// <value>The pipeline builder.</value>
        protected ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> PipelineBuilder { get; }
    }
}