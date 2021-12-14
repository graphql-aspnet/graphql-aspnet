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
    using System.Security.Claims;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A base context defining a set of items supported by all middleware pipelines.
    /// </summary>
    public interface IGraphExecutionContext
    {
        /// <summary>
        /// Marks this context as being cancelled. This does not terminate a pipeline directly, rather it sets a
        /// flag, <see cref="IsCancelled"/>, that each middleware component must choose to react to.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Gets a value indicating whether this resolution context has been put in a canceled
        /// state. Each pipeline component must choose to react to the cancelation state or not. The final data result
        /// of the pipeline, however; will not be rendered to the requestor.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        bool IsCancelled { get; }

        /// <summary>
        /// Gets the original operation request that caused the pipeline to be invoked.
        /// </summary>
        /// <value>The operation request.</value>
        IGraphOperationRequest OperationRequest { get; }

        /// <summary>
        /// Gets the service provider to use for any required object instantiations.
        /// </summary>
        /// <value>The services.</value>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the security context used to authenticate and authorize field executions.
        /// </summary>
        /// <value>The security context.</value>
        IUserSecurityContext SecurityContext { get; }

        /// <summary>
        /// Gets the metrics package attached to this operation, if any.
        /// </summary>
        /// <value>The metrics.</value>
        IGraphQueryExecutionMetrics Metrics { get; }

        /// <summary>
        /// Gets the logger instance assigned to this context.
        /// </summary>
        /// <value>The logger.</value>
        IGraphEventLogger Logger { get; }

        /// <summary>
        /// Gets a key/value pair collection that can be used to store
        /// items used at different parts of processing the request. This collection is user driven
        /// and not used by the graphql library.
        /// </summary>
        /// <value>The metadata.</value>
        MetaDataCollection Items { get; }

        /// <summary>
        /// Gets the collected set of query messages generated during this context's lifetime.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets a value indicating whether this context is still in a valid and runnable state.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }
    }
}