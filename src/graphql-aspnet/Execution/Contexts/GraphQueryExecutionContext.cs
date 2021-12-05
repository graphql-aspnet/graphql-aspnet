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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Claims;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Middleware;

    /// <summary>
    /// A top level context for the execution of a query through the runtime. Functions similarly to how HttpContext works for
    /// an HttpRequest.
    /// </summary>
    [DebuggerDisplay("IsValid = {IsValid} (Messages = {Messages.Count})")]
    public class GraphQueryExecutionContext : BaseGraphExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQueryExecutionContext" /> class.
        /// </summary>
        /// <param name="request">The request to be processed through the query pipeline.</param>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="securityContext">The security context used to authenticate and
        /// authorize fields on this execution.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        /// <param name="items">A key/value pair collection for random access data.</param>
        public GraphQueryExecutionContext(
            IGraphOperationRequest request,
            IServiceProvider serviceProvider,
            IUserSecurityContext securityContext = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            MetaDataCollection items = null)
            : base(request, serviceProvider, securityContext, metrics, logger, items)
        {
            this.FieldResults = new List<GraphDataItem>();
            this.PostProcessingActions = new List<Action>();
            this.DefaultFieldSources = new DefaultFieldSourceCollection();
        }

        /// <summary>
        /// Gets or sets the operation result created during the query pipeline execution.
        /// </summary>
        /// <value>The result.</value>
        public IGraphOperationResult Result { get; set; }

        /// <summary>
        /// Gets or sets the active query plan this context will use to complete the request.
        /// </summary>
        /// <value>The query plan.</value>
        public IGraphQueryPlan QueryPlan { get; set; }

        /// <summary>
        /// Gets or sets the syntax tree parsed from the provided query text. Will be null if a query plan
        /// was retrieved from the cache.
        /// </summary>
        /// <value>The syntax tree.</value>
        public ISyntaxTree SyntaxTree { get; set; }

        /// <summary>
        /// Gets or sets the query operation to execute of the active query plan.
        /// </summary>
        /// <value>The query operation.</value>
        public IGraphFieldExecutableOperation QueryOperation { get; set; }

        /// <summary>
        /// Gets the collection of top level field results produced by executing the operation on this
        /// context.
        /// </summary>
        /// <value>The top level field results.</value>
        public IList<GraphDataItem> FieldResults { get; }

        /// <summary>
        /// Gets a list of registered actions to be executed after processing is complete.
        /// </summary>
        /// <value>The post processing actions.</value>
        public IList<Action> PostProcessingActions { get; }

        /// <summary>
        /// Gets a collection of source objects that can, if needed, be used as the source input values to a
        /// field execution when no other sources exist.
        /// </summary>
        /// <value>The default field sources.</value>
        public DefaultFieldSourceCollection DefaultFieldSources { get; }
    }
}