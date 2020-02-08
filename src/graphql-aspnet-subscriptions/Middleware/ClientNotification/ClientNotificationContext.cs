// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.ClientNotification
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// A middleware context for the client notification pipeline, in which one client is
    /// notified of a new event.
    /// </summary>
    public class ClientNotificationContext : BaseGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotificationContext"/> class.
        /// </summary>
        /// <param name="otherContext">The other context.</param>
        protected ClientNotificationContext(IGraphMiddlewareContext otherContext)
            : base(otherContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotificationContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="user">The user.</param>
        /// <param name="metrics">The metrics.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="items">The items.</param>
        protected ClientNotificationContext(
            IServiceProvider serviceProvider,
            ClaimsPrincipal user = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
            : base(serviceProvider, user, metrics, logger, items)
        {
        }
    }
}