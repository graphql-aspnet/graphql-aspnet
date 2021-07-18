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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// An interface representing an object acting as the runtime for the core graphql
    /// engine for a given schema. This runtime accepts requests and renders responses, nothing else.
    /// </summary>
    public interface IGraphQLRuntime
    {
        /// <summary>
        /// Builds the primary request context used to execute the query and generate a response for this runtime
        /// instance.
        /// </summary>
        /// <param name="queryData">The data package contaning the raw values
        /// that need to be packaged. When null an empty request is generated.</param>
        /// <returns>A fully qualified request context that can be executed.</returns>
        IGraphOperationRequest CreateRequest(GraphQueryData queryData = null);

        /// <summary>
        /// Creates a new metrics package using the default means available to this runtime instance.
        /// </summary>
        /// <returns>Task&lt;IGraphQueryExecutionMetrics&gt;.</returns>
        IGraphQueryExecutionMetrics CreateMetricsPackage();

        /// <summary>
        /// Accepts a query context to execute and renders the result.
        /// </summary>
        /// <param name="context">The execution context to process.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task&lt;IGraphOperationResult&gt;.</returns>
        Task<IGraphOperationResult> ExecuteRequest(
            GraphQueryExecutionContext context,
            CancellationToken cancelToken = default);

        /// <summary>
        /// Accepts a qualified operation request and renders the result using the provided user details.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for resolving
        /// graph objects during execution.</param>
        /// <param name="user">The claims principal representing the user to authorize
        /// on the query.</param>
        /// <param name="request">The primary data request.</param>
        /// <param name="metricsPackage">An optional metrics package to populate during the run.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task&lt;IGraphOperationResult&gt;.</returns>
        Task<IGraphOperationResult> ExecuteRequest(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user,
            IGraphOperationRequest request,
            IGraphQueryExecutionMetrics metricsPackage = null,
            CancellationToken cancelToken = default);

        /// <summary>
        /// Accepts a qualified operation request and renders the result.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for resolving
        /// graph objects.</param>
        /// <param name="user">The claims principal representing the user to authorize
        /// on the query.</param>
        /// <param name="request">The primary data request.</param>
        /// <param name="enableMetrics">if set to <c>true</c> a metrics package will be created and automatically
        /// supplied to the execution request.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task&lt;IGraphOperationResult&gt;.</returns>
        Task<IGraphOperationResult> ExecuteRequest(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user,
            IGraphOperationRequest request,
            bool enableMetrics = false,
            CancellationToken cancelToken = default);
    }
}