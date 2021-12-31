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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A builder for constructing hte individual pipelines the schema will use when executing a query.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this builder exists for.</typeparam>
    public partial class SchemaBuilder<TSchema> : ISchemaBuilder<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaBuilder{TSchema}" /> class.
        /// </summary>
        /// <param name="options">The primary options for configuring the schema.</param>
        public SchemaBuilder(SchemaOptions options)
        {
            Validation.ThrowIfNull(options, nameof(options));

            this.FieldExecutionPipeline = new SchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(options, Constants.Pipelines.FIELD_EXECUTION_PIPELINE);
            this.FieldAuthorizationPipeline = new SchemaPipelineBuilder<TSchema, IGraphFieldSecurityMiddleware, GraphFieldSecurityContext>(options, Constants.Pipelines.FIELD_AUTHORIZATION_PIPELINE);
            this.QueryExecutionPipeline = new SchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext>(options, Constants.Pipelines.QUERY_PIPELINE);
            this.DirectiveExecutionPipeline = new SchemaPipelineBuilder<TSchema, IGraphDirectiveExecutionMiddleware, GraphDirectiveExecutionContext>(options, Constants.Pipelines.DIRECTIVE_PIPELINE);

            this.Options = options;
        }

        /// <inheritdoc cref="ISchemaBuilder{TSchema}.QueryExecutionPipeline" />
        public SchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> QueryExecutionPipeline { get; }

        /// <inheritdoc cref="ISchemaBuilder{TSchema}.FieldExecutionPipeline" />
        public SchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> FieldExecutionPipeline { get; }

        /// <inheritdoc cref="ISchemaBuilder{TSchema}.FieldAuthorizationPipeline" />
        public SchemaPipelineBuilder<TSchema, IGraphFieldSecurityMiddleware, GraphFieldSecurityContext> FieldAuthorizationPipeline { get; }

        /// <inheritdoc cref="ISchemaBuilder{TSchema}.DirectiveExecutionPipeline" />
        public SchemaPipelineBuilder<TSchema, IGraphDirectiveExecutionMiddleware, GraphDirectiveExecutionContext> DirectiveExecutionPipeline { get; }

        /// <inheritdoc />
        ISchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> ISchemaBuilder<TSchema>.FieldExecutionPipeline => this.FieldExecutionPipeline;

        /// <inheritdoc />
        ISchemaPipelineBuilder<TSchema, IGraphFieldSecurityMiddleware, GraphFieldSecurityContext> ISchemaBuilder<TSchema>.FieldAuthorizationPipeline => this.FieldAuthorizationPipeline;

        /// <inheritdoc />
        ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, GraphQueryExecutionContext> ISchemaBuilder<TSchema>.QueryExecutionPipeline => this.QueryExecutionPipeline;

        /// <inheritdoc />
        ISchemaPipelineBuilder<TSchema, IGraphDirectiveExecutionMiddleware, GraphDirectiveExecutionContext> ISchemaBuilder<TSchema>.DirectiveExecutionPipeline => this.DirectiveExecutionPipeline;

        /// <summary>
        /// Gets the completed options used to configure this schema.
        /// </summary>
        /// <value>The options.</value>
        public SchemaOptions Options { get; }
    }
}