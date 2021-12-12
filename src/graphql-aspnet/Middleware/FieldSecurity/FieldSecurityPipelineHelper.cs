// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldSecurity
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldSecurity.Components;

    /// <summary>
    /// A wrapper for a schema pipeline builder to easily add the default middleware componentry for
    /// the field authorizatio pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the pipeline is being constructed for.</typeparam>
    public class FieldSecurityPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, IGraphFieldSecurityMiddleware, GraphFieldSecurityContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSecurityPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public FieldSecurityPipelineHelper(ISchemaPipelineBuilder<TSchema, IGraphFieldSecurityMiddleware, GraphFieldSecurityContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldSecurityPipelineHelper<TSchema> AddDefaultMiddlewareComponents(Configuration.SchemaOptions options = null)
        {
            return this.AddFieldAuthenticationMiddleware()
                       .AddFieldAuthorizationMiddleware();
        }

        /// <summary>
        /// Adds the middleware component that performs authentication for the field.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldSecurityPipelineHelper<TSchema> AddFieldAuthenticationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<FieldAuthenticationMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that performs the primary field authorization.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldSecurityPipelineHelper<TSchema> AddFieldAuthorizationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<FieldAuthorizationMiddleware>();
            return this;
        }
    }
}