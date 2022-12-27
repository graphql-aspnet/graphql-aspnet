// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using System.Threading;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// An execution context defining a set of items supported by all
    /// middleware pipelines.
    /// </summary>
    public interface IGraphQLMiddlewareExecutionContext
    {
        /// <summary>
        /// Marks this context as being cancelled. This does not terminate a pipeline directly, rather it sets a
        /// flag that each middleware component must choose to react to.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Gets a value indicating whether this resolution context has been put in a canceled
        /// state either by the runtime itself, via a call to <see cref="Cancel"/> or via the governing
        /// <see cref="CancellationToken"/>. Each pipeline component must choose to react to
        /// the cancelation state or not. The final data result of the pipeline, however; will
        /// not be rendered to the requestor when the execution context is in a cancelled state.
        /// </summary>
        /// <remarks>
        /// <see cref="IsCancelled"/> and <see cref="IsValid"/> states are independent.  A context can be valid but still
        /// while being cancelled.
        /// </remarks>
        /// <value><c>true</c> if execution of the context has been canceled; otherwise, <c>false</c>.</value>
        bool IsCancelled { get; }

        /// <summary>
        /// Gets the top level request that caused the pipeline to be invoked, typically
        /// generated from an HTTP request.
        /// </summary>
        /// <value>The top level request.</value>
        IQueryExecutionRequest QueryRequest { get; }

        /// <summary>
        /// Gets the service provider assigned to this context to use for any required
        /// object instantiations.
        /// </summary>
        /// <value>The services.</value>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the security context used for authentication and authorizion operations
        /// against secure items.
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, it is assumed that an "anonymous request" is being processed.
        /// </remarks>
        /// <value>The security context.</value>
        IUserSecurityContext SecurityContext { get; }

        /// <summary>
        /// Gets the metrics package attached to this operation, if any.
        /// </summary>
        /// <value>The metrics package.</value>
        IQueryExecutionMetrics Metrics { get; }

        /// <summary>
        /// Gets the logger instance assigned to this context. All events written to the logger will
        /// be automatically contextualized and scoped to the request.
        /// </summary>
        /// <value>The logger instance this context should write events to, if any.</value>
        IGraphEventLogger Logger { get; }

        /// <summary>
        /// Gets the collected set of query messages generated during this context's lifetime.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets a value indicating whether this context is still in a valid and runnable state.
        /// </summary>
        /// <remarks>
        /// <see cref="IsCancelled"/> and <see cref="IsValid"/> states are independent.  A context can be valid but still
        /// while being cancelled.
        /// </remarks>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets or sets a cancellation token that context should obey as well as provide
        /// to developer code for various operations.
        /// </summary>
        /// <value>The governing cancellation token.</value>
        CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets a set of user-driven key/value pairs shared between all contexts during a
        /// single query execution.
        /// </summary>
        /// <remarks>
        /// This is a collection for developer use and is not used by graphql.
        /// </remarks>
        /// <value>The items collection by this query execution.</value>
        MetaDataCollection Items { get; }

        /// <summary>
        /// Gets the object used to track runtime session data for a single query.
        /// </summary>
        /// <remarks>
        /// This is an internal entity reserved for use by graphql's pipelines and
        /// should not be utilized upon by user/developer code. Modification of the data within
        /// the session can cause the query execution to break.
        /// </remarks>
        /// <value>The active query session.</value>
        IQuerySession Session { get; }
    }
}