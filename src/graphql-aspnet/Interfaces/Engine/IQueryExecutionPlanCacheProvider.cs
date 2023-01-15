// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// An interface describing the query plan cache. Build your own cache against any technology you wish
    /// and subsitute it in the <see cref="GraphQLProviders"/> at start up. This cache instance is a singleton reference
    /// per server instance.
    /// </summary>
    public interface IQueryExecutionPlanCacheProvider
    {
        /// <summary>
        /// Attempts to retrieve a query plan from the cache for the given schema if it exists.
        /// </summary>
        /// <param name="key">The unique key for the plan of a given schema.</param>
        /// <returns>Returns the query plan if it was found, otherwise null.</returns>
        Task<IQueryExecutionPlan> TryGetPlanAsync(string key);

        /// <summary>
        /// Caches the plan instance for later retrieval.
        /// </summary>
        /// <param name="key">The unique key for the plan of a given schema.</param>
        /// <param name="plan">The plan to cache.</param>
        /// <param name="absoluteExpiration">The absolute date, in UTC-0 time, on which the plan will expire and be
        /// ejected from the cache. (may not be supported by all cache implementations).</param>
        /// <param name="slidingExpiration">A sliding expiration such that if the plan is not retreived within this timeframe
        /// the plan will be evicted from the cache (may not be supported by all cache implementations).</param>
        /// <returns><c>true</c> if the plan was successfully cached, <c>false</c> otherwise.</returns>
        Task<bool> TryCachePlanAsync(string key, IQueryExecutionPlan plan, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

        /// <summary>
        /// Immediately evicts the query plan with the given key from the cache. If no plan is found, no action is taken.
        /// </summary>
        /// <param name="key">The unique key for the plan of a given schema.</param>
        /// <returns><c>true</c> if the plan was successfully evicted, <c>false</c> otherwise.</returns>
        Task<bool> EvictAsync(string key);
    }
}