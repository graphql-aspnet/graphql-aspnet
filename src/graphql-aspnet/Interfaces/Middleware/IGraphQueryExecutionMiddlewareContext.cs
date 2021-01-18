// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Middleware
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// An interface representing a core context required to complete the primary
    /// query execution pipeline.
    /// </summary>
    public interface IGraphQueryExecutionMiddlewareContext : IGraphMiddlewareContext
    {
        /// <summary>
        /// Gets the query text defining the query document to be executed.
        /// </summary>
        /// <value>The query text.</value>
        IGraphOperationRequest Request { get; }

        /// <summary>
        /// Gets or sets the active query plan this context will use to complete the request.
        /// </summary>
        /// <value>The query plan.</value>
        IGraphQueryPlan QueryPlan { get; set; }

        /// <summary>
        /// Gets or sets the syntax tree parsed from the provided query text. Will be null if a query plan
        /// was retrieved from the cache.
        /// </summary>
        /// <value>The syntax tree.</value>
        ISyntaxTree SyntaxTree { get; set; }

        /// <summary>
        /// Gets or sets the query operation to execute of the active query plan.
        /// </summary>
        /// <value>The query operation.</value>
        IGraphFieldExecutableOperation QueryOperation { get; set; }

        /// <summary>
        /// Gets or sets the operation result created during the query pipeline execution.
        /// </summary>
        /// <value>The result.</value>
        IGraphOperationResult Result { get; set; }

        /// <summary>
        /// Gets the collection of top level field results produced by executing the operation on this
        /// context.
        /// </summary>
        /// <value>The top level field results.</value>
        IList<GraphDataItem> FieldResults { get; }

        /// <summary>
        /// Gets a list of registered actions to be executed after processing is complete.
        /// </summary>
        /// <value>The post processing actions.</value>
        IList<Action> PostProcessingActions { get; }

        /// <summary>
        /// Gets a collection of source objects that can, if needed, be used as the source input values to a
        /// field execution when no other sources exist.
        /// </summary>
        /// <value>The default field sources.</value>
        DefaultFieldSourceCollection DefaultFieldSources { get; }
    }
}