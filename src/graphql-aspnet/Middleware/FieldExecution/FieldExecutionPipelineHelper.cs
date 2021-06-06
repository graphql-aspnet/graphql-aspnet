// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldExecution
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldExecution.Components;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A wrapper for a schema pipeline builder to easily add the default middleware componentry for
    /// the field execution pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the pipeline is being constructed for.</typeparam>
    public class FieldExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldExecutionPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public FieldExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>GraphQL.AspNet.Middleware.FieldExecution.FieldExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            this.AddValidateContextMiddleware();

            var authOption = options?.AuthorizationOptions?.Method ?? AuthorizationMethod.PerField;
            if (authOption == AuthorizationMethod.PerField)
            {
                this.AddFieldAuthorizationMiddleware();
            }

            this.AddResolveDirectivesMiddleware();
            this.AddResolveFieldMiddleware();
            this.AddChildFieldProcessingMiddleware();

            return this;
        }

        /// <summary>
        /// Adds the middleware component to invoke/resolve each directive on a field request.
        /// </summary>
        /// <returns>FieldExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldExecutionPipelineHelper<TSchema> AddResolveDirectivesMiddleware()
        {
            _pipelineBuilder.AddMiddleware<InvokeDirectiveResolversMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component to resolve the field context by invoking the assigned field resolver.
        /// </summary>
        /// <returns>FieldExecutionPipelineHelper.</returns>
        public FieldExecutionPipelineHelper<TSchema> AddResolveFieldMiddleware()
        {
            _pipelineBuilder.AddMiddleware<InvokeFieldResolverMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component to validate the context before executing the pipeline.
        /// </summary>
        /// <returns>FieldExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldExecutionPipelineHelper<TSchema> AddValidateContextMiddleware()
        {
            _pipelineBuilder.AddMiddleware<ValidateFieldExecutionMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component to process any down-stream/child fields processes.
        /// </summary>
        /// <returns>FieldExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldExecutionPipelineHelper<TSchema> AddChildFieldProcessingMiddleware()
        {
            _pipelineBuilder.AddMiddleware<ProcessChildFieldsMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component to authorize the user on the context to the field being processed.
        /// </summary>
        /// <returns>FieldExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldExecutionPipelineHelper<TSchema> AddFieldAuthorizationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<AuthorizeFieldMiddleware<TSchema>>();
            return this;
        }
    }
}