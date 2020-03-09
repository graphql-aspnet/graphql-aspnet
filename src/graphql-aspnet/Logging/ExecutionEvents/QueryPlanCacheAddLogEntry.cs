// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when an executor successfully caches a newly created query plan to its
    /// local cache for future use.
    /// </summary>
    public class QueryPlanCacheAddLogEntry : GraphLogEntry
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryPlanCacheAddLogEntry" /> class.
        /// </summary>
        /// <param name="queryHash">The query hash.</param>
        /// <param name="queryPlan">The query plan.</param>
        public QueryPlanCacheAddLogEntry(string queryHash, IGraphQueryPlan queryPlan)
            : base(LogEventIds.QueryCacheAdd)
        {
            this.QueryPlanHashCode = queryHash;
            _schemaTypeShortName = queryPlan.SchemaType.FriendlyName();
            this.SchemaTypeName = queryPlan.SchemaType.FriendlyName(true);
            this.QueryPlanId = queryPlan.Id;
        }

        /// <summary>
        /// Gets the hash code that was used, and found, in the query cache.
        /// </summary>
        /// <value>The query plan hash code.</value>
        public string QueryPlanHashCode
        {
            get => this.GetProperty<string>(LogPropertyNames.QUERY_PLAN_HASH_CODE);
            private set => this.SetProperty(LogPropertyNames.QUERY_PLAN_HASH_CODE, value);
        }

        /// <summary>
        /// Gets the <see cref="Type" /> name of the target schema for
        /// the query plan.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the unique id assigned to the query plan when it was generated.
        /// </summary>
        /// <value>The query plan identifier.</value>
        public string QueryPlanId
        {
            get => this.GetProperty<string>(LogPropertyNames.QUERY_PLAN_ID);
            private set => this.SetProperty(LogPropertyNames.QUERY_PLAN_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Query Cache Add | Schema Type: '{_schemaTypeShortName}', Key: '[Redacted]' ";
        }
    }
}