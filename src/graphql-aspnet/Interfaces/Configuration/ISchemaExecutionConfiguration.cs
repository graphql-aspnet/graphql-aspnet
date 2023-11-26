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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;

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
        /// processing (Default: Not Set).
        /// </summary>
        /// <value>The total amount of time to wait for a query to finish before issuing a cancellation.</value>
        TimeSpan? QueryTimeout { get; }

        /// <summary>
        /// <para>
        /// Gets a set of options indicating which resolver types will be executed in isolation. Resolvers
        /// executed in isolation will be executed in a manner such that no other resolvers of any kind will be
        /// executing when they are executed.
        /// </para>
        /// <para>
        /// Executing resolvers in isolation can help to alleviate some race conditions encountered
        /// by various injected services that must be scoped but not thread safe.</para>
        /// <para>
        /// If <see cref="DebugMode"/> is enabled all resolvers
        /// will be executed in isolation regardless of this setting.</para>
        /// </summary>
        /// <value>The resolver isolation options.</value>
        ResolverIsolationOptions ResolverIsolation { get; }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether debug mode is enabled. When enabled,
        /// if a field resolution results in an exception that exception is immediately thrown and
        /// query execution is terminated. This can be useful to surface errors to the console window or
        /// output stream while developing.
        /// </para>
        /// <para>
        /// Debug mode should not be enabled in production, doing so can result in
        /// inconsistant results being delivered to a requestor.
        /// </para>
        /// </summary>
        /// <value><c>true</c> if debug mode is enabled; otherwise, <c>false</c>.</value>
        bool DebugMode { get; }

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
        float? MaxQueryComplexity { get; }

        /// <summary>
        /// Gets a value that determines the behaviort of the runtime when a required parameter
        /// to a field or directive resolver cannot be found.
        /// </summary>
        /// <remarks>
        /// Required refers to a method parameter that does not supply a default value.
        /// </remarks>
        /// <value>The resolver parameter resolution rule.</value>
        ResolverParameterResolutionRules ResolverParameterResolutionRule { get; }
    }
}