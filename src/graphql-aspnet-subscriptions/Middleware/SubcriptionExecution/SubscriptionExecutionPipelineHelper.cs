// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubcriptionExecution
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;

    /// <summary>
    /// A decorator for the query execution pipeline builder to configure default components.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper will add components for.</typeparam>
    public class SubscriptionExecutionPipelineHelper<TSchema> : QueryExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionExecutionPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public SubscriptionExecutionPipelineHelper(
            ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> pipelineBuilder)
            : base(pipelineBuilder)
        {
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public new SubscriptionExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            this.AddValidateRequestMiddleware()
                .AddRecordQueryMetricsMiddleware();

            if (options?.CacheOptions != null && !options.CacheOptions.Disabled)
                this.AddQueryPlanCacheMiddleware();

            this.AddQueryDocumentParsingMiddleware()
                .AddQueryPlanCreationMiddleware()
                .AddQueryAssignOperationMiddleware();

            if (options == null || options.AuthorizationOptions.Method == Security.AuthorizationMethod.PerRequest)
            {
                this.AddQueryOperationAuthorizationMiddleware();
            }

            this.AddSubscriptionCreationMiddleware();

            this
                .AddQueryPlanExecutionMiddleware()
                .AddResultCreationMiddleware();

            return this;
        }

        /// <summary>
        /// Adds a middleware component that will create a subscription and halt query execution
        /// if the chosen operation is a subscription operation. The subscription will be added to the metadata collection
        /// as <see cref="SubscriptionConstants.Execution.CREATED_SUBSCRIPTION"/>.
        /// </summary>
        /// <returns>SubscriptionQueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public SubscriptionExecutionPipelineHelper<TSchema> AddSubscriptionCreationMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<SubscriptionCreationMiddleware<TSchema>>();
            return this;
        }
    }
}