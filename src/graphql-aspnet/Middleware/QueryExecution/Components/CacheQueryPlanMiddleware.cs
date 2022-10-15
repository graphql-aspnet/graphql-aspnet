﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.QueryExecution.Components
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Attempts to retrieve a query plan from the global cache and assign it to the active context.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the pipeline is being constructed for.</typeparam>
    public class CacheQueryPlanMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly IGraphQueryPlanCacheKeyManager _keyManager;
        private readonly IGraphQueryPlanCacheProvider _cacheProvider;
        private readonly ISchemaQueryPlanCacheConfiguration _cacheOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheQueryPlanMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public CacheQueryPlanMiddleware(TSchema schema)
             : this(schema, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheQueryPlanMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="keyManager">The key manager.</param>
        /// <param name="cacheProvider">The cache provider.</param>
        public CacheQueryPlanMiddleware(TSchema schema, IGraphQueryPlanCacheKeyManager keyManager, IGraphQueryPlanCacheProvider cacheProvider)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            _cacheOptions = schema.Configuration.QueryCacheOptions;

            _keyManager = keyManager;
            _cacheProvider = cacheProvider;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            bool planFound = false;
            string key = null;
            if (_keyManager != null && _cacheProvider != null && context.OperationRequest?.QueryText != null)
            {
                key = _keyManager.CreateKey<TSchema>(
                    context.OperationRequest.QueryText,
                    context.OperationRequest.OperationName);

                planFound = await _cacheProvider.TryGetPlanAsync(key, out var queryPlan).ConfigureAwait(false);
                if (planFound)
                {
                    context.QueryPlan = queryPlan;
                    context.Logger?.QueryPlanCacheFetchHit<TSchema>(key);
                }
                else
                {
                    context.Logger?.QueryPlanCacheFetchMiss<TSchema>(key);
                }
            }

            // invoke the rest of the pipeline
            await next(context, cancelToken).ConfigureAwait(false);

            // try and cache the completed plan
            if (!planFound
                && !string.IsNullOrWhiteSpace(key)
                && context.QueryPlan != null
                && context.QueryPlan.IsValid
                && context.QueryPlan.IsCacheable)
            {
                // calculate the plan's time to live in the cache and cache it
                DateTimeOffset? absoluteExpiration = null;
                TimeSpan? slidingExpiration = null;
                if (_cacheOptions.TimeToLiveInMilliseconds.HasValue)
                {
                    absoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(_cacheOptions.TimeToLiveInMilliseconds.Value);
                }
                else if (_cacheOptions.SlidingExpiration.HasValue)
                {
                    slidingExpiration = _cacheOptions.SlidingExpiration.Value;
                }

                var successful = await _cacheProvider.TryCachePlanAsync(
                    key,
                    context.QueryPlan,
                    absoluteExpiration,
                    slidingExpiration).ConfigureAwait(false);

                if (successful)
                    context.Logger?.QueryPlanCached(key, context.QueryPlan);
            }
        }
    }
}