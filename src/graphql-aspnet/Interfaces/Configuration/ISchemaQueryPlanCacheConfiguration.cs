// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System;

    /// <summary>
    /// A collection of configuration options for a given schema on how to configure query plan caching.
    /// </summary>
    public interface ISchemaQueryPlanCacheConfiguration
    {
        /// <summary>
        /// Gets or sets the total number of milliseconds from the point a query is cached until it is removed from the cache. This value takes precidence over
        /// <see cref="SlidingExpiration" /> if set. (Default: Not Set).
        /// </summary>
        /// <value>The time to live in minutes.</value>
        ulong? TimeToLiveInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a sliding expiration such that if the query has not been requested from the cache in this amount of time it will be
        /// removed from the cache. (Default: 30 minutes).
        /// </summary>
        /// <value>The sliding expiration.</value>
        TimeSpan? SlidingExpiration { get; set; }
    }
}