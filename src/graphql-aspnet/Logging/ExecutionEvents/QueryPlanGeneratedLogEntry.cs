﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when an executor finishes creating a query plan and is ready to
    /// cache and execute against it.
    /// </summary>
    public class QueryPlanGeneratedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryPlanGeneratedLogEntry" /> class.
        /// </summary>
        /// <param name="queryPlan">The query plan.</param>
        public QueryPlanGeneratedLogEntry(IGraphQueryPlan queryPlan)
            : base(LogEventIds.QueryPlanGenerationCompleted)
        {
            this.SchemaTypeName = queryPlan?.SchemaType?.FriendlyName(true);
            this.QueryPlanIsValid = queryPlan?.IsValid;
            this.QueryPlanEstimatedComplexity = queryPlan?.EstimatedComplexity;
            this.QueryPlanOperationName = queryPlan?.OperationName;
            this.QueryPlanId = queryPlan?.Id.ToString();
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
        /// Gets a value indicating whether the query plan being logged is valid and
        /// can be executed against.
        /// </summary>
        /// <value><c>true</c> if the query plan is valid; otherwise, <c>false</c>.</value>
        public bool? QueryPlanIsValid
        {
            get => this.GetProperty<bool?>(LogPropertyNames.QUERY_PLAN_IS_VALID);
            private set => this.SetProperty(LogPropertyNames.QUERY_PLAN_IS_VALID, value);
        }

        /// <summary>
        /// Gets the depth of the deepest field resolution path for any operation in a query plan.
        /// </summary>
        /// <value>The query plan maximum depth.</value>
        public int? QueryPlanMaxDepth
        {
            get => this.GetProperty<int?>(LogPropertyNames.QUERY_PLAN_MAX_DEPTH);
            private set => this.SetProperty(LogPropertyNames.QUERY_PLAN_MAX_DEPTH, value);
        }

        /// <summary>
        /// Gets the estimated complexity of the query plan as calculated by the target
        /// schema.
        /// </summary>
        /// <value>The query plan maximum depth.</value>
        public float? QueryPlanEstimatedComplexity
        {
            get => this.GetProperty<float?>(LogPropertyNames.QUERY_PLAN_ESTIMATED_COMPLEXITY);
            private set => this.SetProperty(LogPropertyNames.QUERY_PLAN_ESTIMATED_COMPLEXITY, value);
        }

        /// <summary>
        /// Gets the name of the operation attached to the query plan.
        /// </summary>
        /// <value>The name of the query plan operation.</value>
        public string QueryPlanOperationName
        {
            get => this.GetProperty<string>(LogPropertyNames.QUERY_PLAN_OPERATION_NAME);
            private set => this.SetProperty(LogPropertyNames.QUERY_PLAN_OPERATION_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.QueryPlanId?.Length > 8 ? this.QueryPlanId.Substring(0, 8) : this.QueryPlanId;
            return $"Query Plan Generated | Id: {idTruncated}, Max Depth: {this.QueryPlanMaxDepth}, Complexity: {this.QueryPlanEstimatedComplexity}";
        }
    }
}