// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldAuthorization;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubscriptionEventExecution;

    /// <summary>
    /// A decorator on the primary <see cref="ISchemaBuilder{TSchema}"/> to add a new pipeline for subscriptions.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class SchemaBuilderWithSubscriptionSupport<TSchema> : ISubscriptionSchemaBuilder<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaBuilder<TSchema> _rootBuilder;
        private readonly ISchemaPipelineBuilder<TSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> _subscriptionPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaBuilderWithSubscriptionSupport{TSchema}" /> class.
        /// </summary>
        /// <param name="rootBuilder">The root builder.</param>
        /// <param name="subscriptionPipelineBuilder">The subscription pipeline being constructed.</param>
        public SchemaBuilderWithSubscriptionSupport(
            ISchemaBuilder<TSchema> rootBuilder,
            ISchemaPipelineBuilder<TSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> subscriptionPipelineBuilder)
        {
            _rootBuilder = Validation.ThrowIfNullOrReturn(rootBuilder, nameof(_rootBuilder));
            this.SubscriptionExecutionPipeline = Validation.ThrowIfNullOrReturn(subscriptionPipelineBuilder, nameof(subscriptionPipelineBuilder));
        }

        /// <summary>
        /// Gets the completed options used to configure this schema.
        /// </summary>
        /// <value>The options.</value>
        public SchemaOptions Options => _rootBuilder.Options;

        /// <summary>
        /// Gets a builder to construct the subscription execution pipeline. This pipeline is invoked per event raised per
        /// registered subscription to process event data and ultimately transmit said to a connected client.
        /// </summary>
        /// <value>The field execution pipeline.</value>
        public ISchemaPipelineBuilder<TSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> SubscriptionExecutionPipeline
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a builder to construct the field execution pipeline. This pipeline is invoked per field resolution request to generate a
        /// piece of data in the process of fulfilling the primary query.
        /// </summary>
        /// <value>The field execution pipeline.</value>
        public ISchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> FieldExecutionPipeline
        {
            get => _rootBuilder.FieldExecutionPipeline;
        }

        /// <summary>
        /// Gets a builder to construct the field authorization pipeline. This pipeline is invoked per field resolution request to authorize
        /// the user to the field allowing or denying them access to it.
        /// </summary>
        /// <value>The field authorization pipeline.</value>
        public ISchemaPipelineBuilder<TSchema, IGraphFieldAuthorizationMiddleware, GraphFieldAuthorizationContext> FieldAuthorizationPipeline
        {
            get => _rootBuilder.FieldAuthorizationPipeline;
        }

        /// <summary>
        /// Gets a builder to construct the primary query pipeline. This pipeline oversees the processing of a query and is invoked
        /// directly by the http handler.
        /// </summary>
        /// <value>The query execution pipeline.</value>
        public ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> QueryExecutionPipeline
        {
            get => _rootBuilder.QueryExecutionPipeline;
        }
    }
}