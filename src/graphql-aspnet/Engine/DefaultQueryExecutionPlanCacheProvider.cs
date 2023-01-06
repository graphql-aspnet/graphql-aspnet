// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// The default query cache implementation using local memory to store query plan data.
    /// </summary>
    public class DefaultQueryExecutionPlanCacheProvider : IQueryExecutionPlanCacheProvider, IDisposable
    {
        /// <summary>
        /// The number of minutes to use as a default sliding expiration on any cached plans
        /// when both a cache instance and query plan policy does not declare one explicitly.
        /// </summary>
        public const int DEFAULT_SLIDING_EXPIRATION_MINUTES = 15;

        private readonly MemoryCache _cachedPlans;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryExecutionPlanCacheProvider"/> class.
        /// </summary>
        /// <param name="defaultSlidingExpiration">The default sliding expiration.</param>
        public DefaultQueryExecutionPlanCacheProvider(TimeSpan? defaultSlidingExpiration = null)
             : this(MemoryCache.Default, defaultSlidingExpiration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryExecutionPlanCacheProvider" /> class.
        /// </summary>
        /// <param name="cacheInstance">The cache instance to use for storing query plans.</param>
        /// <param name="defaultSlidingExpiration">The default sliding expiration.</param>
        public DefaultQueryExecutionPlanCacheProvider(MemoryCache cacheInstance, TimeSpan? defaultSlidingExpiration = null)
        {
            _cachedPlans = cacheInstance;
            if (defaultSlidingExpiration.HasValue)
                this.DefaultSlidingExpiration = defaultSlidingExpiration.Value;
            else
                this.DefaultSlidingExpiration = TimeSpan.FromMinutes(DEFAULT_SLIDING_EXPIRATION_MINUTES);
        }

        /// <inheritdoc />
        public Task<bool> EvictAsync(string key)
        {
            _cachedPlans.Remove(key);
            return true.AsCompletedTask();
        }

        /// <inheritdoc />
        public Task<IQueryExecutionPlan> TryGetPlanAsync(string key)
        {
            var plan = _cachedPlans.Get(key) as IQueryExecutionPlan;
            return Task.FromResult(plan as IQueryExecutionPlan);
        }

        /// <inheritdoc />
        public Task<bool> TryCachePlanAsync(string key, IQueryExecutionPlan plan, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
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

        /// <inheritdoc />
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