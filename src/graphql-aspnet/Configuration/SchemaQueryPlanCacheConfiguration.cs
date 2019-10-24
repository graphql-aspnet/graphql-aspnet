// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// A collection of configuration options for a given schema on how to configure query plan caching.
    /// </summary>
    public class SchemaQueryPlanCacheConfiguration : ISchemaQueryPlanCacheConfiguration
    {
        /// <summary>
        /// Merges the specified configuration setttings into this instance.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void Merge(ISchemaQueryPlanCacheConfiguration config)
        {
            if (config == null)
                return;

            this.TimeToLiveInMilliseconds = config.TimeToLiveInMilliseconds;
            this.SlidingExpiration = config.SlidingExpiration;
        }

        /// <summary>
        /// Gets or sets the total number of milliseconds from the point a query is cached until it is removed from the cache. This value takes precidence over
        /// <see cref="SlidingExpiration" /> if set. (Default: Not Set).
        /// </summary>
        /// <value>The time to live in minutes.</value>
        public ulong? TimeToLiveInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a sliding expiration such that if the query has not been requested from the cache in this amount of time it will be
        /// removed from the cache. (Default: 30 minutes).
        /// </summary>
        /// <value>The sliding expiration.</value>
        public TimeSpan? SlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// <para>Gets or sets a value indicating whether this query caching is turned off for this schema. (Default: false).</para>
        /// <para>Note: This option is dependent on enabling the query cache at startup. If the query
        /// cache is not enabled for the application it is automatically disabled.. See the documentation
        /// for further details.</para>
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Disabled { get; set; }
    }
}