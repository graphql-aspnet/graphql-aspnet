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
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// Represents a generated response to a query.
    /// </summary>
    public interface IGraphOperationResult
    {
        /// <summary>
        /// Gets the original operation request that was executed to produce this result.
        /// </summary>
        /// <value>The request.</value>
        IGraphOperationRequest Request { get; }

        /// <summary>
        /// Gets the resultant data item that was generated as a result of completing the operation.
        /// </summary>
        /// <value>The data item that was generated.</value>
        IResponseFieldSet Data { get; }

        /// <summary>
        /// Gets the sum total collection of messages generated at the various executed phases of
        /// processing. These messages may indicate an error or just be informational in nature.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the collection of metrics that were generated during the execution of the operation.
        /// </summary>
        /// <value>The metrics.</value>
        IGraphQueryExecutionMetrics Metrics { get; }

        /// <summary>
        /// Gets or sets a value indicating whether any exceptions thrown as a result of a graph operation
        /// should be included in the response generated to be sent to the requestor.
        /// WARNING: Setting this property to true in a production environment could pose a security risk..
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed to clients; otherwise, <c>false</c>.</value>
        bool ExposeExceptions { get; set; }
    }
}