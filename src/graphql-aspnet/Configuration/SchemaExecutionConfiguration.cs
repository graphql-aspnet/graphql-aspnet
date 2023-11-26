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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A set of configuration options that will be applied to the execution of a graphql query through
    /// this <see cref="ISchema" />.
    /// </summary>
    [DebuggerDisplay("Schema Execution Configuration")]
    public class SchemaExecutionConfiguration : ISchemaExecutionConfiguration
    {
        private ResolverIsolationOptions _isolationOptions = ResolverIsolationOptions.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaExecutionConfiguration"/> class.
        /// </summary>
        public SchemaExecutionConfiguration()
        {
            this.ResolverParameterResolutionRule = ResolverParameterResolutionRules.ThrowException;
        }

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
            this.ResolverIsolation = config.ResolverIsolation;
            this.MaxQueryDepth = config.MaxQueryDepth;
            this.MaxQueryComplexity = config.MaxQueryComplexity;
            this.DebugMode = config.DebugMode;
            this.ResolverParameterResolutionRule = config.ResolverParameterResolutionRule;
        }

        /// <inheritdoc />
        public bool EnableMetrics { get; set; }

        /// <inheritdoc />
        public TimeSpan? QueryTimeout { get; set; }

        /// <inheritdoc />
        public int? MaxQueryDepth { get; set; }

        /// <inheritdoc />
        public float? MaxQueryComplexity { get; set; }

        /// <inheritdoc />
        public bool DebugMode { get; set; }

        /// <inheritdoc />
        public ResolverParameterResolutionRules ResolverParameterResolutionRule { get; set; }

        /// <inheritdoc />
        public ResolverIsolationOptions ResolverIsolation
        {
            get
            {
                if (this.DebugMode)
                    return ResolverIsolationOptions.All;

                return _isolationOptions;
            }

            set
            {
                _isolationOptions = value;
            }
        }
    }
}