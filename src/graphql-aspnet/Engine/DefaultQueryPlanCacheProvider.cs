// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// The default query cache implementation using local memory to store query plan data.
    /// </summary>
    public class DefaultQueryPlanCacheProvider : IGraphQueryPlanCacheProvider, IDisposable
    {
        /// <summary>
        /// The number of minutes to use as a default sliding expiration on any cached plans
        /// when both a cache instance and query plan policy does not declare one explicitly.
        /// </summary>
        public const int DEFAULT_SLIDING_EXPIRATION_MINUTES = 15;

        private readonly MemoryCache _cachedPlans;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryPlanCacheProvider"/> class.
        /// </summary>
        /// <param name="defaultSlidingExpiration">The default sliding expiration.</param>
        public DefaultQueryPlanCacheProvider(TimeSpan? defaultSlidingExpiration = null)
             : this(MemoryCache.Default, defaultSlidingExpiration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryPlanCacheProvider" /> class.
        /// </summary>
        /// <param name="cacheInstance">The cache instance to use for storing query plans.</param>
        /// <param name="defaultSlidingExpiration">The default sliding expiration.</param>
        public DefaultQueryPlanCacheProvider(MemoryCache cacheInstance, TimeSpan? defaultSlidingExpiration = null)
        {
            _cachedPlans = cacheInstance;
            if (defaultSlidingExpiration.HasValue)
                this.DefaultSlidingExpiration = defaultSlidingExpiration.Value;
            else
                this.DefaultSlidingExpiration = TimeSpan.FromMinutes(DEFAULT_SLIDING_EXPIRATION_MINUTES);
        }

        /// <summary>
        /// Immediately evicts the query plan from the cache.
        /// </summary>
        /// <param name="key">The unique hash for the plan of a given schema.</param>
        /// <returns><c>true</c> if the plan was successfully evicted, <c>false</c> otherwise.</returns>
        public Task<bool> EvictAsync(string key)
        {
            _cachedPlans.Remove(key);
            return true.AsCompletedTask();
        }

        /// <summary>
        /// Attempts to retrieve a query plan from the cache for the given schema if it sexists.
        /// </summary>
        /// <param name="key">The unique hash for the plan of a given schema.</param>
        /// <param name="plan">The plan that was retrieved or null if it was not found.</param>
        /// <returns><c>true</c> if the plan was successfully retrieved; otherwise, <c>false</c>.</returns>
        /// where TSchema : class, ISchema
        public Task<bool> TryGetPlanAsync(string key, out IGraphQueryPlan plan)
        {
            plan = _cachedPlans.Get(key) as IGraphQueryPlan;
            return (plan != null).AsCompletedTask();
        }

        /// <summary>
        /// Caches the plan instance for later retrieval.
        /// </summary>
        /// <param name="key">The unique hash for the plan of a given schema.</param>
        /// <param name="plan">The plan to cache.</param>
        /// <param name="absoluteExpiration">The absolute date, in UTC-0 time, on which the plan will expire and be
        /// ejected from the cache. (may not be supported by all cache implementations).</param>
        /// <param name="slidingExpiration">A sliding expiration such that if the plan is not retreived within this timeframe
        /// the plan will be evicted from the cache (may not be supported by all cache implementations).</param>
        /// <returns><c>true</c> if the plan was successfully cached, <c>false</c> otherwise.</returns>
        public Task<bool> TryCachePlanAsync(string key, IGraphQueryPlan plan, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var policy = new CacheItemPolicy();
            if (absoluteExpiration.HasValue)
            {
                policy.AbsoluteExpiration = absoluteExpiration.Value;
            }
            else if (slidingExpiration.HasValue)
            {
                policy.SlidingExpiration = slidingExpiration.Value;
            }
            else
            {
                policy.SlidingExpiration = this.DefaultSlidingExpiration;
            }

            _cachedPlans.Set(key, plan, policy);
            return true.AsCompletedTask();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_cachedPlans != MemoryCache.Default)
                    _cachedPlans.Dispose();
            }
        }

        /// <summary>
        /// Gets or sets the default sliding expiration policy value to use when a cached plan
        /// does not provide one explicitly.
        /// </summary>
        /// <value>The default sliding expiration.</value>
        protected TimeSpan DefaultSlidingExpiration { get; set; }

        /// <summary>
        /// Gets the number of plans cached.
        /// </summary>
        /// <value>The count.</value>
        public long Count => _cachedPlans.GetCount();
    }
}