﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Interfaces.Execution.Response;

    /// <summary>
    /// Represents a generated response to a graphql query.
    /// </summary>
    public interface IQueryExecutionResult
    {
        /// <summary>
        /// Gets the original request that was executed to produce this result.
        /// </summary>
        /// <value>The original request.</value>
        IQueryExecutionRequest QueryRequest { get; }

        /// <summary>
        /// Gets the resultant, top-level data item that was generated during the operation. This is the
        /// "data" field value that is generated by every graphql query.
        /// </summary>
        /// <value>The data item that was generated.</value>
        IQueryResponseFieldSet Data { get; }

        /// <summary>
        /// Gets the collection of messages generated during query
        /// processing. These messages may indicate an error or just be informational in nature.
        /// </summary>
        /// <value>The messages generated during processing.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the collection of metrics that were generated during the execution of the operation.
        /// May be null if metrics are not enabled.
        /// </summary>
        /// <value>The metrics generated during the execution of the request.</value>
        IQueryExecutionMetrics Metrics { get; }

        /// <summary>
        /// Gets or sets a value indicating whether any exceptions thrown as a result of a graph operation
        /// should be included in the response generated to be sent to the requestor.
        /// WARNING: Setting this property to true in a production environment could pose a security risk..
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed to clients; otherwise, <c>false</c>.</value>
        bool ExposeExceptions { get; set; }
    }
}