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
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// A base middleware context containing the core items required of all contexts.
    /// </summary>
    public abstract class BaseGraphMiddlewareContext : IGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphMiddlewareContext"/> class.
        /// </summary>
        /// <param name="otherContext">The other context.</param>
        protected BaseGraphMiddlewareContext(IGraphMiddlewareContext otherContext)
            : this(
                    otherContext.ServiceProvider,
                    otherContext.User,
                    otherContext.Metrics,
                    otherContext.Logger,
                    otherContext.Items)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphMiddlewareContext" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="user">The user authenticated by the Asp.net runtime.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        protected BaseGraphMiddlewareContext(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
        {
            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
            this.User = user;
            this.Metrics = metrics;
            this.Items = items ?? new MetaDataCollection();
            this.Messages = new GraphMessageCollection();
            this.Logger = logger;
        }

        /// <summary>
        /// Marks this context as being cancelled. This does not terminate a pipeline directly, rather it sets a
        /// flag, <see cref="IsCancelled"/>, that each middleware component must choose to react to.
        /// </summary>
        public void Cancel()
        {
            this.IsCancelled = true;
        }

        /// <summary>
        /// Gets a value indicating whether this resolution context has been put in a canceled
        /// state. Each pipeline component must choose to react to the cancelation state or not. The final data result
        /// of the pipeline, however; will not be rendered to the requestor.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// Gets the service provider to use for any required object instantiations.
        /// </summary>
        /// <value>The services.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the user context executing this request.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets the metrics package attached to this operation, if any.
        /// </summary>
        /// <value>The metrics.</value>
        public IGraphQueryExecutionMetrics Metrics { get; }

        /// <summary>
        /// Gets a key/value pair collection that can be used to store
        /// items used at different parts of processing the request. This collection is user driven
        /// and not used by the graphql library.
        /// </summary>
        /// <value>The metadata.</value>
        public MetaDataCollection Items { get; }

        /// <summary>
        /// Gets the logger instance assigned to this context.
        /// </summary>
        /// <value>The logger.</value>
        public IGraphEventLogger Logger { get; }

        /// <summary>
        /// Gets the collected set of query messages generated during this context's lifetime.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets a value indicating whether this context is still in a valid and runnable state.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => !this.Messages.Severity.IsCritical();
    }
}