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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A base context used by all field and directive resolution contexts in order to successfully invoke
    /// a controller action, object method or object property and retrieve a data value for a field.
    /// </summary>
    public abstract class SchemaItemResolutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemResolutionContext" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider instance that should be used to resolve any
        /// needed services or non-schema arguments to the target resolver.</param>
        /// <param name="querySession">The query session governing the request.</param>
        /// <param name="targetSchema">The schema in scope for this resolution context.</param>
        /// <param name="queryRequest">The master query request being executed.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="messages">(Optional) A collection of messages that can be written to during resolution. These messages
        /// will be transmitted to the requestor.</param>
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
        {
            this.Session = Validation.ThrowIfNullOrReturn(querySession, nameof(querySession));
            this.ServiceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
            this.QueryRequest = Validation.ThrowIfNullOrReturn(queryRequest, nameof(queryRequest));
            this.Request = Validation.ThrowIfNullOrReturn(request, nameof(request));
            this.ExecutionSuppliedArguments = Validation.ThrowIfNullOrReturn(arguments, nameof(arguments));
            this.User = user;
            this.Logger = logger;
            this.Schema = Validation.ThrowIfNullOrReturn(targetSchema, nameof(targetSchema));
            this.ExecutionSuppliedArguments = this.ExecutionSuppliedArguments.ForContext(this);
            this.Messages = messages ?? new GraphMessageCollection();
            this.CancellationToken = cancelToken;
        }

        /// <summary>
        /// Cancels this resolution context indicating it did not complete successfully.
        /// </summary>
        public virtual void Cancel()
        {
            this.IsCancelled = true;
        }

        /// <summary>
        /// Gets a value indicating whether this resolution context was canceled, indicating it did not complete
        /// its resolution operation successfully.
        /// </summary>
        /// <value><c>true</c> if this instance is canceled; otherwise, <c>false</c>.</value>
        public virtual bool IsCancelled { get; private set; }

        /// <summary>
        /// Gets the set of argument, if any, to be supplied to the method the resolver will call to
        /// complete its operation.
        /// </summary>
        /// <value>The arguments.</value>
        public IExecutionArgumentCollection ExecutionSuppliedArguments { get; }

        /// <summary>
        /// Gets a collection of messages that be written to. These messages will be transmitted to the requestor.
        /// </summary>
        /// <value>The message collection available to this context.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the cancellation token governing this resolution. Any raised cancel requests via this token should be obeyed.
        /// </summary>
        /// <value>The cancellation token governing the resolution of the target schema item.</value>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a service provider instance that can be used to resolve services during this schema item's resolution cycle.
        /// </summary>
        /// <value>The service provider instance available for resolution of services.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the master query request that was initially supplied to the runtime.
        /// </summary>
        /// <value>The query request.</value>
        public IQueryExecutionRequest QueryRequest { get; }

        /// <summary>
        /// Gets the request governing the resolver's operation.
        /// </summary>
        /// <value>The request.</value>
        public IDataRequest Request { get; }

        /// <summary>
        /// Gets the resolved user that was authenticated and authorized for this resolution context.
        /// May be null if no authentication or authorization took place.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets a logger instance can be written to to record scoped log entries.
        /// </summary>
        /// <value>The logger.</value>
        public IGraphEventLogger Logger { get; }

        /// <summary>
        /// Gets the schema that is targeted by this context.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }

        /// <summary>
        /// Gets the path to the item being resolved.
        /// </summary>
        /// <value>The path of the item being resolved.</value>
        public abstract ItemPath ItemPath { get; }

        /// <summary>
        /// Gets the set of arguments defined on the schema that are to be resolved to fulfill this request.
        /// </summary>
        /// <value>The set of arguments to use in resolution.</value>
        public abstract IGraphArgumentCollection SchemaDefinedArguments { get; }

        /// <summary>
        /// Gets the source data item resolved from a parent resolver, if any. May be null.
        /// </summary>
        /// <value>The source data item supplied to this context.</value>
        public abstract object SourceData { get; }

        /// <summary>
        /// Gets the object used to track runtime session data for a single query.
        /// </summary>
        /// <remarks>
        /// This is an internal entity reserved for use by graphql's pipelines and
        /// should not be utilized upon by controller action methods. Modification of the data within
        /// the session can cause the query execution to break.
        /// </remarks>
        /// <value>The active query session.</value>
        public IQuerySession Session { get; }
    }
}