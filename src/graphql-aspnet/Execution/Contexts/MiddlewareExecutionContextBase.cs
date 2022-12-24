// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Contexts
{
    using System;
    using System.Threading;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A base middleware context containing the core items required of all contexts.
    /// </summary>
    public abstract class MiddlewareExecutionContextBase : IMiddlewareExecutionContext
    {
        private bool _isCancelled;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareExecutionContextBase"/> class.
        /// </summary>
        /// <param name="otherContext">The other context on which this context is based.</param>
        protected MiddlewareExecutionContextBase(IMiddlewareExecutionContext otherContext)
            : this(
                    otherContext?.OperationRequest,
                    otherContext?.ServiceProvider,
                    otherContext?.Session,
                    otherContext?.SecurityContext,
                    otherContext?.Items,
                    otherContext?.Metrics,
                    otherContext?.Logger,
                    otherContext?.CancellationToken ?? default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareExecutionContextBase" /> class.
        /// </summary>
        /// <param name="operationRequest">The original operation request.</param>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="querySession">The query session governing the execution of a query.</param>
        /// <param name="securityContext">The security context used to authenticate
        /// and authorize fields on this execution context.</param>
        /// <param name="items">A collection of developer-driven items for tracking various pieces of data.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="cancelToken">The cancel token governing this execution context.</param>
        protected MiddlewareExecutionContextBase(
            IQueryOperationRequest operationRequest,
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            IUserSecurityContext securityContext = null,
            MetaDataCollection items = null,
            IQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            CancellationToken cancelToken = default)
        {
            this.OperationRequest = Validation.ThrowIfNullOrReturn(operationRequest, nameof(operationRequest));
            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
            this.Session = Validation.ThrowIfNullOrReturn(querySession, nameof(Session));

            this.SecurityContext = securityContext;
            this.Metrics = metrics;
            this.Messages = new GraphMessageCollection();
            this.Items = items ?? new MetaDataCollection();
            this.Logger = logger;

            this.CancellationToken = cancelToken;
        }

        /// <inheritdoc />
        public virtual void Cancel()
        {
            _isCancelled = true;
        }

        /// <inheritdoc />
        public bool IsCancelled => _isCancelled || this.CancellationToken.IsCancellationRequested;

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext { get; }

        /// <inheritdoc />
        public IQueryExecutionMetrics Metrics { get; }

        /// <inheritdoc />
        public IGraphEventLogger Logger { get; }

        /// <inheritdoc />
        public IGraphMessageCollection Messages { get; }

        /// <inheritdoc />
        public bool IsValid => !this.Messages.Severity.IsCritical();

        /// <inheritdoc />
        public IQueryOperationRequest OperationRequest { get; }

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; }

        /// <inheritdoc />
        public IQuerySession Session { get; }

        /// <inheritdoc />
        public MetaDataCollection Items { get; }
    }
}