// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Middleware.DirectiveExecution
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.DirectiveExecution.Components;

    /// <summary>
    /// A wrapper for a schema pipeline builder to easily add the default middleware componentry for
    /// the directive execution pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the pipeline is being constructed for.</typeparam>
    public class DirectiveExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, IGraphDirectiveExecutionMiddleware, GraphDirectiveExecutionContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveExecutionPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public DirectiveExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, IGraphDirectiveExecutionMiddleware, GraphDirectiveExecutionContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>DirectiveExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public DirectiveExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            this.AddValidateContextMiddleware();
            this.AddResolverDirectiveMiddleware();

            return this;
        }

        /// <summary>
        /// Adds the middleware component to resolve the field context by invoking the assigned field resolver.
        /// </summary>
        /// <returns>DirectiveExecutionPipelineHelper.</returns>
        public DirectiveExecutionPipelineHelper<TSchema> AddResolverDirectiveMiddleware()
        {
            _pipelineBuilder.AddMiddleware<InvokeDirectiveResolverMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component to validate the context before executing the pipeline.
        /// </summary>
        /// <returns>DirectiveExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public DirectiveExecutionPipelineHelper<TSchema> AddValidateContextMiddleware()
        {
            _pipelineBuilder.AddMiddleware<ValidateDirectiveExecutionMiddleware<TSchema>>();
            return this;
        }
    }
}