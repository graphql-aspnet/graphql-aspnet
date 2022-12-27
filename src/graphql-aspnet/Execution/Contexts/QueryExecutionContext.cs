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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A top level context for the execution of a query through the graphql runtime.
    /// </summary>
    /// <remarks>
    /// This context plays a role similar to the HttpContext that governs web requests
    /// through asp.net.
    /// </remarks>
    [DebuggerDisplay("IsValid = {IsValid} (Messages = {Messages.Count})")]
    public class QueryExecutionContext : MiddlewareExecutionContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutionContext" /> class.
        /// </summary>
        /// <param name="request">The request to be processed through the query pipeline.</param>
        /// <param name="serviceProvider">The service provider passed on the HttpContext.</param>
        /// <param name="querySession">The query session governing the execution of a query.</param>
        /// <param name="items">(optional) A collection of developer-driven items for tracking various pieces of data.
        /// If not provided a new collection will be created.</param>
        /// <param name="securityContext">(optional) The security context used to authenticate and
        /// authorize fields on this execution. If not provided this query will be executed
        /// with an "anonymous" context.</param>
        /// <param name="metrics">(optional) The metrics package used to profile this request.
        /// If not supplied, no profiling will take place.</param>
        /// <param name="logger">(optional) The logger instance to record events related to this context.
        /// If not provided, no logging events will be recorded.</param>
        public QueryExecutionContext(
            IQueryExecutionRequest request,
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            MetaDataCollection items = null,
            IUserSecurityContext securityContext = null,
            IQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null)
            : base(request, serviceProvider, querySession, securityContext, items, metrics, logger)
        {
            this.FieldResults = new List<FieldDataItem>();
            this.DefaultFieldSources = new FieldSourceCollection();
        }

        /// <summary>
        /// Gets or sets the operation result created during the query pipeline execution.
        /// </summary>
        /// <value>The final result containing the data produced during the query.</value>
        public IQueryExecutionResult Result { get; set; }

        /// <summary>
        /// Gets or sets the active query plan this context will use to complete the request.
        /// </summary>
        /// <value>The query plan.</value>
        public IQueryExecutionPlan QueryPlan { get; set; }

        /// <summary>
        /// Gets or sets the query document, lexed and parsed from the query text
        /// supplied by the user on the request.
        /// </summary>
        /// <value>The completed query document.</value>
        public IQueryDocument QueryDocument { get; set; }

        /// <summary>
        /// Gets or sets the chosen operation, extracted from the <see cref="QueryDocument"/> to
        /// execute. This operation will have its execution directives applied and a query plan
        /// will be generated from it.
        /// </summary>
        /// <value>The chosen operation to execute.</value>
        public IOperationDocumentPart Operation { get; set; }

        /// <summary>
        /// Gets the collection of the top level field results produced by executing the
        /// operation on this context.
        /// </summary>
        /// <value>The top level field results.</value>
        public IList<FieldDataItem> FieldResults { get; }

        /// <summary>
        /// Gets a collection of source objects that can, if needed, supply as the source input
        /// values to various field executions when no other sources exist.
        /// </summary>
        /// <value>The default field sources.</value>
        public FieldSourceCollection DefaultFieldSources { get; }

        /// <summary>
        /// Gets or sets a collection of resolved variable data used to use during the
        /// execution of a query.
        /// </summary>
        /// <value>The collection of resolved variables.</value>
        public IResolvedVariableCollection ResolvedVariables { get; set; }
    }
}