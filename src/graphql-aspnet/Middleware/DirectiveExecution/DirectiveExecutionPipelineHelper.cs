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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Middleware.DirectiveExecution.Components;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A wrapper for a schema pipeline builder to easily add the default middleware componentry for
    /// the directive execution pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the pipeline is being constructed for.</typeparam>
    public class DirectiveExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, IDirectiveExecutionMiddleware, GraphDirectiveExecutionContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveExecutionPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public DirectiveExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, IDirectiveExecutionMiddleware, GraphDirectiveExecutionContext> pipelineBuilder)
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

            var authOption = options?.AuthorizationOptions?.Method ?? AuthorizationMethod.PerField;
            if (authOption == AuthorizationMethod.PerField)
            {
                this.AddAuthorizationMiddleware();
            }

            this.AddResolverMiddleware()
                .AddLoggingMiddleware();

            return this;
        }

        /// <summary>
        /// Adds the middleware component to resolve the field context by invoking the assigned field resolver.
        /// </summary>
        /// <returns>DirectiveExecutionPipelineHelper.</returns>
        public DirectiveExecutionPipelineHelper<TSchema> AddResolverMiddleware()
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

        /// <summary>
        /// Adds the middleware component to authorize the user on the context to the directive being processed.
        /// </summary>
        /// <returns>FieldExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public DirectiveExecutionPipelineHelper<TSchema> AddAuthorizationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<AuthorizeDirectiveMiddleware<TSchema>>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component used to log the successful completion of
        /// appling a directive to a target.
        /// </summary>
        /// <returns>DirectiveExecutionPipelineHelper&lt;TSchema&gt;.</returns>
        public DirectiveExecutionPipelineHelper<TSchema> AddLoggingMiddleware()
        {
            _pipelineBuilder.AddMiddleware<LogDirectiveExecutionMiddleware<TSchema>>();
            return this;
        }
    }
}