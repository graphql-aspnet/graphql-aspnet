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
    using System.Security.Claims;
    using System.Threading;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A typed context used by all field and directive resolution contexts to resolve
    /// a field value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to be resolved.</typeparam>
    public abstract class SchemaItemResolutionContext<TRequest> : SchemaItemResolutionContext
        where TRequest : class, IDataRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemResolutionContext{TRequest}" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider instance that should be used to resolve any
        /// needed services or non-schema arguments to the target resolver.</param>
        /// <param name="querySession">The query session governing the request.</param>
        /// <param name="targetSchema">The schema in scope for this resolution context.</param>
        /// <param name="queryRequest">The master query request being executed.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="logger">(Optional) A logger instance that can be used to record scoped log entries.</param>
        /// <param name="user">(Optional) The user context that authenticated and authorized for this
        /// resolution context.</param>
        /// <param name="cancelToken">The cancel token governing the resolution of the schema item.</param>
        protected SchemaItemResolutionContext(
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            ISchema targetSchema,
            IQueryExecutionRequest queryRequest,
            IDataRequest request,
            IExecutionArgumentCollection arguments,
            IGraphMessageCollection messages = null,
            IGraphEventLogger logger = null,
            ClaimsPrincipal user = null,
            CancellationToken cancelToken = default)
            : base(
                  serviceProvider,
                  querySession,
                  targetSchema,
                  queryRequest,
                  request,
                  arguments,
                  messages,
                  logger,
                  user,
                  cancelToken)
        {
        }

        /// <summary>
        /// Gets the resolution request on this context.
        /// </summary>
        /// <value>The request being resolved.</value>
        public new TRequest Request => base.Request as TRequest;
    }
}