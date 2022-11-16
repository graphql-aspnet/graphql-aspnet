// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SchemaItemSecurity
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.SchemaItemSecurity.Components;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A wrapper for a schema pipeline builder to easily add the default middleware componentry for
    /// the field authorizatio pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the pipeline is being constructed for.</typeparam>
    public class SchemaItemSecurityPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, ISchemaItemSecurityMiddleware, GraphSchemaItemSecurityChallengeContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemSecurityPipelineHelper{TSchema}"/> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public SchemaItemSecurityPipelineHelper(ISchemaPipelineBuilder<TSchema, ISchemaItemSecurityMiddleware, GraphSchemaItemSecurityChallengeContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }

        /// <summary>
        /// Adds all default middleware components, in standard order, to the pipeline.
        /// </summary>
        /// <param name="options">The configuration options to use when deriving the components to include.</param>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public SchemaItemSecurityPipelineHelper<TSchema> AddDefaultMiddlewareComponents(Configuration.SchemaOptions options = null)
        {
            return this.AddGateKeeperMiddleware()
                       .AddPolicyAggregationMiddleware()
                       .AddAuthenticationMiddleware()
                       .AddAuthorizationMiddleware();
        }

        /// <summary>
        /// Adds the middleware component that performs the primary field authorization.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public SchemaItemSecurityPipelineHelper<TSchema> AddPolicyAggregationMiddleware()
        {
            SchemItemSecurityRequirementsMiddleware MiddlewareFactory(IServiceProvider sp)
            {
                // policy provider may not be registered and is optional
                var policyProvider = sp.GetService<IAuthorizationPolicyProvider>();
                return new SchemItemSecurityRequirementsMiddleware(policyProvider);
            }

            _pipelineBuilder.AddMiddleware<SchemItemSecurityRequirementsMiddleware>(
                MiddlewareFactory,
                ServiceLifetime.Singleton);

            return this;
        }

        /// <summary>
        /// Adds the middleware component that performs authentication for the field.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public SchemaItemSecurityPipelineHelper<TSchema> AddAuthenticationMiddleware()
        {
            SchemaItemAuthenticationMiddleware MiddlewareFactory(IServiceProvider sp)
            {
                // policy provider may not be registered and is optional
                var schemeProvider = sp.GetService<IAuthenticationSchemeProvider>();
                return new SchemaItemAuthenticationMiddleware(schemeProvider);
            }

            _pipelineBuilder.AddMiddleware<SchemaItemAuthenticationMiddleware>(
                MiddlewareFactory,
                ServiceLifetime.Singleton);

            return this;
        }

        /// <summary>
        /// Adds the middleware component that performs the primary field authorization.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public SchemaItemSecurityPipelineHelper<TSchema> AddAuthorizationMiddleware()
        {
            _pipelineBuilder.AddMiddleware<SchemaItemAuthorizationMiddleware>();
            return this;
        }

        /// <summary>
        /// Adds the middleware component that performs an initial gate check to ensure
        /// authorization is required before executing the rest of the pipeline.
        /// </summary>
        /// <returns>FieldAuthorizationPipelineHelper&lt;TSchema&gt;.</returns>
        public SchemaItemSecurityPipelineHelper<TSchema> AddGateKeeperMiddleware()
        {
            _pipelineBuilder.AddMiddleware<SchemaItemSecurityGateMiddleware>();
            return this;
        }
    }
}