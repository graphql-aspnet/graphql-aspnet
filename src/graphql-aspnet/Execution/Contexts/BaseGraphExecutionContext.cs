// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A base middleware context containing the core items required of all contexts.
    /// </summary>
    public abstract class BaseGraphExecutionContext : IGraphExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphExecutionContext"/> class.
        /// </summary>
        /// <param name="otherContext">The other context.</param>
        protected BaseGraphExecutionContext(IGraphExecutionContext otherContext)
            : this(
                    otherContext?.OperationRequest,
                    otherContext?.ServiceProvider,
                    otherContext?.SecurityContext,
                    otherContext?.Metrics,
                    otherContext?.Logger,
                    otherContext?.Items)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphExecutionContext" /> class.
        /// </summary>
        /// <param name="operationRequest">The original operation request.</param>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="securityContext">The security context used to authenticate
        /// and authorize fields on this execution context.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        protected BaseGraphExecutionContext(
            IGraphOperationRequest operationRequest,
            IServiceProvider serviceProvider,
            IUserSecurityContext securityContext,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
        {
            this.OperationRequest = Validation.ThrowIfNullOrReturn(operationRequest, nameof(operationRequest));
            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
            this.SecurityContext = securityContext;
            this.Metrics = metrics;
            this.Items = items ?? new MetaDataCollection();
            this.Messages = new GraphMessageCollection();
            this.Logger = logger;
        }

        /// <inheritdoc />
        public void Cancel()
        {
            this.IsCancelled = true;
        }

        /// <inheritdoc />
        public bool IsCancelled { get; private set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IUserSecurityContext SecurityContext { get; }

        /// <inheritdoc />
        public IGraphQueryExecutionMetrics Metrics { get; }

        /// <inheritdoc />
        public MetaDataCollection Items { get; }

        /// <inheritdoc />
        public IGraphEventLogger Logger { get; }

        /// <inheritdoc />
        public IGraphMessageCollection Messages { get; }

        /// <inheritdoc />
        public bool IsValid => !this.Messages.Severity.IsCritical();

        /// <inheritdoc />
        public IGraphOperationRequest OperationRequest { get; }
    }
}