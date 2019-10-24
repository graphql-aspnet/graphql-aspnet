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
    using System.Threading;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of configurations that will be applied to the execution of a graphql query through
    /// this <see cref="ISchema"/>.
    /// </summary>
    public interface ISchemaExecutionConfiguration
    {
        /// <summary>
        /// Gets a value indicating whether data metrics and telemetry should be tracked
        /// as part of every query. Enabling this will result in a performance hit and should be
        /// used for testing and debugging only. (Default: false).
        /// </summary>
        /// <value><c>true</c> if metrics should be globally enabled; otherwise, <c>false</c>.</value>
        bool EnableMetrics { get; }

        /// <summary>
        /// Gets a value indicating the amount of time before a query (or mutation),
        /// executed against this <see cref="ISchema"/>, timesout and is abandoned. A timeout will issue a
        /// cancelation request via a <see cref="CancellationToken"/> but does not garuntee that any running tasks will stop
        /// processing (Default: 1 minute).
        /// </summary>
        /// <value>The total amount of time to wait for a query to finish before issuing a cancellation.</value>
        TimeSpan QueryTimeout { get; }

        /// <summary>
        /// <para>Gets a value indicating whether each field, when its resolved, is individually awaited.</para>
        ///
        /// <para>If this option is set to <c>true</c> each field is executed and awaited individually in depth first order and can make
        /// debugging easier. (Default: false).</para>
        ///
        /// <para>WARNING: Setting this option to <c>true</c> applies a significant performance hit and should only be used during
        /// development.</para>
        /// </summary>
        /// <value><c>true</c> if each field should be individually awaited; otherwise, <c>false</c>.</value>
        bool AwaitEachRequestedField { get; }

        /// <summary>
        /// Gets a value indicating the maximum depth a single query can be and still be processed. Query depth refers to the number
        /// of nested field declaration sets in the query tree. See documentation for further details. (Default: Not Set / Unlimited).
        /// </summary>
        /// <value>An integer representing the maximum depth of a single query before its rejected.</value>
        int? MaxQueryDepth { get; }

        /// <summary>
        /// Gets a value indicating the maximum calculated complexity score for any query plan. Any query plan yielding a complexity score
        /// greater than this value will be rejected and not executed. See documentation for further details. (Default: Not Set / Unlimited).
        /// </summary>
        /// <value>An integer representing the maximum calculated query complexity of a single query plan before its rejected.</value>
        float? MaxQueryComplexity { get;  }
    }
}