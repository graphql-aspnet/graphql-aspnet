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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when an executor attempts, and fails, to retrieve a query plan from its local cache.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for which the query hash was generated.</typeparam>
    public class QueryPlanCacheMissLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryPlanCacheMissLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="queryHash">The query hash.</param>
        public QueryPlanCacheMissLogEntry(string queryHash)
            : base(LogEventIds.QueryCacheMiss)
        {
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.QueryPlanHashCode = queryHash;
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
        /// Gets the <see cref="Type" /> name of the target schema investigated for
        /// the query plan.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Query Cache Miss | Schema Type: '{_schemaTypeShortName}', Key: '[Redacted]' ";
        }
    }
}