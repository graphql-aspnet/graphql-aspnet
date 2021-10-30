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
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of configurations that will be applied to the execution of a graphql query through
    /// this <see cref="ISchema" />.
    /// </summary>
    [DebuggerDisplay("Schema Execution Configuration")]
    public class SchemaExecutionConfiguration : ISchemaExecutionConfiguration
    {
        /// <summary>
        /// Merges the specified configuration setttings into this instance.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void Merge(ISchemaExecutionConfiguration config)
        {
            if (config == null)
                return;

            this.EnableMetrics = config.EnableMetrics;
            this.QueryTimeout = config.QueryTimeout;
            this.AwaitEachRequestedField = config.AwaitEachRequestedField;
            this.MaxQueryDepth = config.MaxQueryDepth;
            this.MaxQueryComplexity = config.MaxQueryComplexity;
        }

        /// <inheritdoc />
        public bool EnableMetrics { get; set; }

        /// <inheritdoc />
        public TimeSpan QueryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <inheritdoc />
        public bool AwaitEachRequestedField { get; set; }

        /// <inheritdoc />
        public int? MaxQueryDepth { get; set; }

        /// <inheritdoc />
        public float? MaxQueryComplexity { get; set; }
    }
}