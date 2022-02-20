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
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldSecurity.Components;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

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
            return this.AddPolicyAggregationMiddleware()
                       .AddFieldAuthenticationMiddleware()
                       .AddFieldAuthorizationMiddleware();
        }

        /// <summary>
        /// Adds the middleware component that performs the primary field authorization.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldSecurityPipelineHelper<TSchema> AddPolicyAggregationMiddleware()
        {
            FieldSecurityRequirementsMiddleware MiddlewareFactory(IServiceProvider sp)
            {
                // policy provider may not be registered and is optional
                var policyProvider = sp.GetService<IAuthorizationPolicyProvider>();
                return new FieldSecurityRequirementsMiddleware(policyProvider);
            }

            _pipelineBuilder.AddMiddleware<FieldSecurityRequirementsMiddleware>(
                MiddlewareFactory,
                ServiceLifetime.Singleton);

            return this;
        }

        /// <summary>
        /// Adds the middleware component that performs authentication for the field.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public FieldSecurityPipelineHelper<TSchema> AddFieldAuthenticationMiddleware()
        {
            FieldAuthenticationMiddleware MiddlewareFactory(IServiceProvider sp)
            {
                // policy provider may not be registered and is optional
                var schemeProvider = sp.GetService<IAuthenticationSchemeProvider>();
                return new FieldAuthenticationMiddleware(schemeProvider);
            }

            _pipelineBuilder.AddMiddleware<FieldAuthenticationMiddleware>(
                MiddlewareFactory,
                ServiceLifetime.Singleton);

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