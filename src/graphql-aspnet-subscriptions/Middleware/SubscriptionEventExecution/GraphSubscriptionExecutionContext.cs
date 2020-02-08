// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubscriptionExecution
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// An execution context carried throughout the subscription execution pipeline to
    /// complete processing of one event received by this subscription server.
    /// </summary>
    public class GraphSubscriptionExecutionContext : BaseGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSubscriptionExecutionContext"/> class.
        /// </summary>
        /// <param name="otherContext">The other context.</param>
        protected GraphSubscriptionExecutionContext(IGraphMiddlewareContext otherContext)
            : base(otherContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSubscriptionExecutionContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="user">The user authenticated by the Asp.net runtime.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        protected GraphSubscriptionExecutionContext(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user = null,
            IGraphQueryExecutionMetrics metrics = null,
            Interfaces.Logging.IGraphEventLogger logger = null,
            MetaDataCollection items = null)
            : base(serviceProvider, user, metrics, logger, items)
        {
        }
    }
}