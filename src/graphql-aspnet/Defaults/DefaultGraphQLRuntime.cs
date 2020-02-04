// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// The default implementation of the core graphql runtime.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this runtime exists for.</typeparam>
    public class DefaultGraphQLRuntime<TSchema> : IGraphQLRuntime<TSchema>
        where TSchema : class, ISchema
    {
        private const string ERROR_NO_RESPONSE = "GraphQL runtime returned no response.";

        private readonly ISchemaPipeline<TSchema, GraphQueryExecutionContext> _pipeline;
        private readonly IGraphEventLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLRuntime{TSchema}" /> class.
        /// </summary>
        /// <param name="pipeline">The pipeline to execute any requests through.</param>
        /// <param name="logger">The logger used to record events during an execution.</param>
        public DefaultGraphQLRuntime(ISchemaPipeline<TSchema, GraphQueryExecutionContext> pipeline, IGraphEventLogger logger = null)
        {
            _pipeline = Validation.ThrowIfNullOrReturn(pipeline, nameof(pipeline));
            _logger = logger;
        }

        /// <summary>
        /// Builds the primary request context used to execute the query and generate a response for this runtime
        /// instance.
        /// </summary>
        /// <param name="queryData">The data package contaning the raw values
        /// that need to be packaged.</param>
        /// <returns>A fully qualified request context that can be executed.</returns>
        public IGraphOperationRequest CreateRequest(GraphQueryData queryData)
        {
            return new GraphOperationRequest(
                queryData.Query,
                queryData.OperationName,
                queryData.Variables);
        }

        /// <summary>
        /// Accepts a qualified operation request and renders the result.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for resolving
        /// graph objects.</param>
        /// <param name="user">The claims principal representing the user to authorize
        ///  on the query.</param>
        /// <param name="request">The primary data request.</param>
        /// <param name="metricsPackage">An optional metrics package to populate during the run.</param>
        /// <returns>Task&lt;IGraphOperationResult&gt;.</returns>
        public async Task<IGraphOperationResult> ExecuteRequest(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user,
            IGraphOperationRequest request,
            IGraphQueryExecutionMetrics metricsPackage = null)
        {
            Validation.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            Validation.ThrowIfNull(user, nameof(user));
            Validation.ThrowIfNull(request, nameof(request));

            using (var cancelSource = new CancellationTokenSource())
            {
                // *******************************
                // Primary query execution
                // *******************************
                var context = new GraphQueryExecutionContext(
                    request,
                    serviceProvider,
                    user,
                    metricsPackage,
                    _logger);

                await _pipeline.InvokeAsync(context, cancelSource.Token).ConfigureAwait(false);

                // *******************************
                // Response Generation
                // *******************************
                var queryResponse = context.Result;
                if (queryResponse == null)
                {
                    queryResponse = new GraphOperationResult(request);
                    queryResponse.Messages.Add(GraphMessageSeverity.Critical, ERROR_NO_RESPONSE, Constants.ErrorCodes.GENERAL_ERROR);
                }

                return queryResponse;
            }
        }
    }
}