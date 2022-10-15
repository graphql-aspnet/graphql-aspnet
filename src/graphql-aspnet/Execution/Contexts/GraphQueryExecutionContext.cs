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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Internal.Interfaces;

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
        /// <param name="querySession">The query session governing the execution of a query.</param>
        /// <param name="items">A collection of developer-driven items for tracking various pieces of data.</param>
        /// <param name="securityContext">The security context used to authenticate and
        /// authorize fields on this execution.</param>
        /// <param name="metrics">The metrics package to profile this request, if any.</param>
        /// <param name="logger">The logger instance to record events related to this context.</param>
        public GraphQueryExecutionContext(
            IGraphOperationRequest request,
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            MetaDataCollection items = null,
            IUserSecurityContext securityContext = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null)
            : base(request, serviceProvider, querySession, securityContext, items, metrics, logger)
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
        /// Gets or sets the query document, parsed from the query text supplied by the user
        /// on the request.
        /// </summary>
        /// <value>The completed query document.</value>
        public IGraphQueryDocument QueryDocument { get; set; }

        /// <summary>
        /// Gets or sets the chosen operation, parsed from the <see cref="QueryDocument"/> to
        /// execute. This operation will have its directives applied and a query plan will be generated
        /// from it.
        /// </summary>
        /// <value>The chosen operation to execute.</value>
        public IOperationDocumentPart Operation { get; set; }

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

        /// <summary>
        /// Gets or sets a collection of resolved variable data used throughout this context.
        /// </summary>
        /// <value>The resolved variables.</value>
        public IResolvedVariableCollection ResolvedVariables { get; set; }
    }
}