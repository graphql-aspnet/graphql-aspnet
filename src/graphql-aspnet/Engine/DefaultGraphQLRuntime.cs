// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// The default implementation of the core graphql runtime responsible for generating
    /// <see cref="IQueryOperationResult"/> from <see cref="IQueryOperationRequest"/>.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this runtime operates with.</typeparam>
    public class DefaultGraphQLRuntime<TSchema> : IGraphQLRuntime<TSchema>
        where TSchema : class, ISchema
    {
        private const string ERROR_NO_RESPONSE = "GraphQL runtime returned no response.";

        private readonly ISchemaPipeline<TSchema, QueryExecutionContext> _pipeline;
        private readonly IGraphEventLogger _logger;
        private readonly IQueryExecutionMetricsFactory<TSchema> _metricsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLRuntime{TSchema}" /> class.
        /// </summary>
        /// <param name="pipeline">The top level pipeline to execute any requests on.</param>
        /// <param name="metricsFactory">The factory to produce metrics packages if and when needed.</param>
        /// <param name="logger">The logger used to record events during an execution.</param>
        public DefaultGraphQLRuntime(
            ISchemaPipeline<TSchema, QueryExecutionContext> pipeline,
            IQueryExecutionMetricsFactory<TSchema> metricsFactory = null,
            IGraphEventLogger logger = null)
        {
            _pipeline = Validation.ThrowIfNullOrReturn(pipeline, nameof(pipeline));
            _logger = logger;
            _metricsFactory = metricsFactory;
        }

        /// <inheritdoc />
        public IQueryExecutionMetrics CreateMetricsPackage()
        {
            return _metricsFactory?.CreateMetricsPackage();
        }

        /// <inheritdoc />
        public IQueryOperationRequest CreateRequest(GraphQueryData queryData = null)
        {
            return new GraphOperationRequest(queryData ?? GraphQueryData.Empty);
        }

        /// <inheritdoc />
        public Task<IQueryOperationResult> ExecuteRequestAsync(
            IServiceProvider serviceProvider,
            IQueryOperationRequest request,
            CancellationToken cancelToken = default)
        {
            Validation.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            Validation.ThrowIfNull(request, nameof(request));

            return this.ExecuteRequestAsync(
                serviceProvider,
                request,
                securityContext: null,
                metricsPackage: null,
                session: null,
                cancelToken: cancelToken);
        }

        /// <inheritdoc />
        public Task<IQueryOperationResult> ExecuteRequestAsync(
            IServiceProvider serviceProvider,
            IQueryOperationRequest request,
            IUserSecurityContext securityContext = null,
            bool enableMetrics = false,
            CancellationToken cancelToken = default)
        {
            Validation.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            Validation.ThrowIfNull(request, nameof(request));

            return this.ExecuteRequestAsync(
                serviceProvider,
                request,
                securityContext: securityContext,
                metricsPackage: enableMetrics ? this.CreateMetricsPackage() : null,
                session: null,
                cancelToken: cancelToken);
        }

        /// <inheritdoc />
        public Task<IQueryOperationResult> ExecuteRequestAsync(
            IServiceProvider serviceProvider,
            IQueryOperationRequest request,
            IUserSecurityContext securityContext = null,
            IQueryExecutionMetrics metricsPackage = null,
            IQuerySession session = null,
            CancellationToken cancelToken = default)
        {
            Validation.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            Validation.ThrowIfNull(request, nameof(request));

            session = session ?? new QuerySession();

            var context = new QueryExecutionContext(
                request,
                serviceProvider,
                session,
                items: new MetaDataCollection(),
                securityContext: securityContext,
                metrics: metricsPackage,
                logger: _logger);

            return this.ExecuteRequestAsync(context, cancelToken);
        }

        /// <inheritdoc />
        public async Task<IQueryOperationResult> ExecuteRequestAsync(
            QueryExecutionContext context,
            CancellationToken cancelToken = default)
        {
            Validation.ThrowIfNull(context, nameof(context));

            context.CancellationToken = cancelToken;

            // *******************************
            // Primary query execution
            // *******************************
            await _pipeline.InvokeAsync(context, cancelToken).ConfigureAwait(false);

            // *******************************
            // Response Generation
            // *******************************
            var queryResponse = context.Result;
            if (queryResponse == null)
            {
                queryResponse = new GraphOperationResult(context.OperationRequest);
                queryResponse.Messages.Add(GraphMessageSeverity.Critical, ERROR_NO_RESPONSE, Constants.ErrorCodes.GENERAL_ERROR);
                context.Result = queryResponse;
            }

            return queryResponse;
        }
    }
}