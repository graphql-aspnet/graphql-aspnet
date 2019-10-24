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

    /// <summary>
    /// A decorator for the query execution pipeline builder to configure default components.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper will add components for.</typeparam>
    public class QueryExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutionPipelineHelper{TSchema}" /> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public QueryExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            this.AddValidateRequestMiddleware()
                .AddRecordQueryMetricsMiddleware();

            if (options?.CacheOptions != null && !options.CacheOptions.Disabled)
                this.AddQueryPlanCacheMiddleware();

            this.AddQueryDocumentParsingMiddleware()
                .AddQueryPlanCreationMiddleware()
                .AddQueryAssignOperationMiddleware();

            if (options.AuthorizationOptions.Method == Security.AuthorizationMethod.PerRequest)
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
        private QueryExecutionPipelineHelper<TSchema> AddQueryOperationAuthorizationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<AuthorizeQueryOperationMiddleware<TSchema>>();
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
            _pipelineBuilder.AddMiddleware(new ValidateQueryRequestMiddleware());
            return this;
        }

        /// <summary>
        /// Adds the middleware component to start/stop the master metrics recording process
        /// for the context.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        private QueryExecutionPipelineHelper<TSchema> AddRecordQueryMetricsMiddleware()
        {
            _pipelineBuilder.AddMiddleware<RecordQueryMetricsMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component for retrieving a query plan from the query cache.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryPlanCacheMiddleware()
        {
            _pipelineBuilder.AddMiddleware<QueryPlanCacheMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component for parsing query text into an AST.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryDocumentParsingMiddleware()
        {
            _pipelineBuilder.AddMiddleware<ParseQueryPlanMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component for generating a query plan from a parsed AST.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryPlanCreationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<GenerateQueryPlanMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that will assign the correct operation, from the query plan, to the active context
        /// for the request.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryAssignOperationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<AssignQueryOperationMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that will execute a query plan for a request.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddQueryPlanExecutionMiddleware()
        {
            _pipelineBuilder.AddMiddleware<ExecuteQueryOperationMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that will generate a final reslt after the plan is executed.
        /// </summary>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public QueryExecutionPipelineHelper<TSchema> AddResultCreationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<PackageQueryResultMiddleware>();
            return this;
        }
    }
}