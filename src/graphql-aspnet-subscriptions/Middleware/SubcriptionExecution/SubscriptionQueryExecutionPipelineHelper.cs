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
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubcriptionExecution.Components;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A decorator for the query execution pipeline builder to configure default components.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper will add components for.</typeparam>
    public class SubscriptionQueryExecutionPipelineHelper<TSchema> : QueryExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionQueryExecutionPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public SubscriptionQueryExecutionPipelineHelper(
            ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> pipelineBuilder)
            : base(pipelineBuilder)
        {
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>QueryExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public new SubscriptionQueryExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            this.AddValidateRequestMiddleware()
                .AddRecordQueryMetricsMiddleware();

            if (options?.CacheOptions != null && !options.CacheOptions.Disabled)
                this.AddQueryPlanCacheMiddleware();

            this.AddQueryDocumentParsingMiddleware()
                .AddQueryPlanCreationMiddleware()
                .AddQueryAssignOperationMiddleware();

            var authOption = options?.AuthorizationOptions?.Method ?? AuthorizationMethod.PerRequest;
            if (authOption == AuthorizationMethod.PerRequest)
            {
                this.AddQueryOperationAuthorizationMiddleware();
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid Authorization Method. The default subscription schema pipeline requires a \"{nameof(AuthorizationMethod.PerRequest)}\" " +
                    $"authorization method. (Current authorization method is \"{options.AuthorizationOptions.Method}\")");
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
        public SubscriptionQueryExecutionPipelineHelper<TSchema> AddSubscriptionCreationMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<SubscriptionCreationMiddleware<TSchema>>();
            return this;
        }
    }
}