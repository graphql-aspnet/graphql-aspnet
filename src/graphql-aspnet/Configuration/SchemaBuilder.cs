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
    using System;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldAuthorization;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// A builder for constructing hte individual pipelines the schema will use when executing a query.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this builder exists for.</typeparam>
    public class SchemaBuilder<TSchema> : ISchemaBuilder<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Occurs when a type reference is set to this configuration section that requires injection into the service collection.
        /// </summary>
        internal event EventHandler<TypeReferenceEventArgs> TypeReferenceAdded;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaBuilder{TSchema}" /> class.
        /// </summary>
        public SchemaBuilder()
        {
            this.FieldExecutionPipeline = new SchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(Constants.Pipelines.FIELD_EXECUTION_PIPELINE);
            this.FieldAuthorizationPipeline = new SchemaPipelineBuilder<TSchema, IGraphFieldAuthorizationMiddleware, GraphFieldAuthorizationContext>(Constants.Pipelines.FIELD_AUTHORIZATION_PIPELINE);
            this.QueryExecutionPipeline = new SchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext>(Constants.Pipelines.QUERY_PIPELINE);

            this.FieldExecutionPipeline.TypeReferenceAdded += this.Pipeline_TypeReferenceAdded;
            this.FieldAuthorizationPipeline.TypeReferenceAdded += this.Pipeline_TypeReferenceAdded;
            this.QueryExecutionPipeline.TypeReferenceAdded += this.Pipeline_TypeReferenceAdded;
        }

        /// <summary>
        /// Handles the TypeReferenceAdded event of the Pipeline control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TypeReferenceEventArgs"/> instance containing the event data.</param>
        private void Pipeline_TypeReferenceAdded(object sender, TypeReferenceEventArgs e)
        {
            this.TypeReferenceAdded?.Invoke(sender, e);
        }

        /// <summary>
        /// Gets a builder to construct the field execution pipeline. This pipeline is invoked per field resolution request to generate a
        /// piece of data in the process of fulfilling the primary query.
        /// </summary>
        /// <value>The field execution pipeline.</value>
        public SchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> FieldExecutionPipeline { get; }

        /// <summary>
        /// Gets a builder to construct the field authorization pipeline. This pipeline is invoked per field resolution request to authorize
        /// the user to the field allowing or denying them access to it.
        /// </summary>
        /// <value>The field authorization pipeline.</value>
        public SchemaPipelineBuilder<TSchema, IGraphFieldAuthorizationMiddleware, GraphFieldAuthorizationContext> FieldAuthorizationPipeline { get; }

        /// <summary>
        /// Gets a builder to construct the primary query pipeline. This pipeline oversees the processing of a query and is invoked
        /// directly by the http handler.
        /// </summary>
        /// <value>The query execution pipeline.</value>
        public SchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> QueryExecutionPipeline { get; }

        /// <summary>
        /// Gets a builder to construct the field execution pipeline. This pipeline is invoked per field resolution request to generate a
        /// piece of data in the process of fulfilling the primary query.
        /// </summary>
        /// <value>The field execution pipeline.</value>
        ISchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> ISchemaBuilder<TSchema>.FieldExecutionPipeline => this.FieldExecutionPipeline;

        /// <summary>
        /// Gets a builder to construct the field authorization pipeline. This pipeline is invoked per field resolution request to authorize
        /// the user to the field allowing or denying them access to it.
        /// </summary>
        /// <value>The field authorization pipeline.</value>
        ISchemaPipelineBuilder<TSchema, IGraphFieldAuthorizationMiddleware, GraphFieldAuthorizationContext> ISchemaBuilder<TSchema>.FieldAuthorizationPipeline => this.FieldAuthorizationPipeline;

        /// <summary>
        /// Gets a builder to construct the primary query pipeline. This pipeline oversees the processing of a query and is invoked
        /// directly by the http handler.
        /// </summary>
        /// <value>The query execution pipeline.</value>
        ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> ISchemaBuilder<TSchema>.QueryExecutionPipeline => this.QueryExecutionPipeline;
    }
}