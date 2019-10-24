// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// The default implementation of the object returned at the end of a graph operation.
    /// </summary>
    /// <seealso cref="GraphQL.AspNet.Interfaces.Execution.IGraphOperationResult" />
    [DebuggerDisplay("Messages = {Messages.Count}, Has Data = {HasData}")]
    public class GraphOperationResult : IGraphOperationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperationResult" /> class.
        /// </summary>
        /// <param name="originalRequest">The original request.</param>
        /// <param name="messages">The message collection containing any messages that may
        /// have been generated during execution.</param>
        /// <param name="dataItem">The top level field set resolved during the operation.</param>
        /// <param name="metrics">The metrics package that was filled during the operation execution.</param>
        public GraphOperationResult(
            IGraphOperationRequest originalRequest,
            IEnumerable<IGraphMessage> messages = null,
            IResponseFieldSet dataItem = null,
            IGraphQueryExecutionMetrics metrics = null)
        {
            this.Request = originalRequest;
            this.Data = dataItem;
            this.Messages = new GraphMessageCollection();
            if (messages != null)
                this.Messages.AddRange(messages);

            this.Metrics = metrics;
        }

        /// <summary>
        /// Gets the original operation request that was executed to produce this result.
        /// </summary>
        /// <value>The request.</value>
        public IGraphOperationRequest Request { get; }

        /// <summary>
        /// Gets the resultant data item that was generated as a result of completing the operation.
        /// </summary>
        /// <value>The data item that was generated.</value>
        public IResponseFieldSet Data { get; }

        /// <summary>
        /// Gets the sum total collection of messages generated at the various executed phases of
        /// processing. These messages may indicate an error or just be informational in nature.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets or sets a value indicating whether any exceptions thrown as a result of a graph operation
        /// should be included in the response generated to be sent to the requestor.
        /// WARNING: Setting this property to true in a production environment could pose a security risk.
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed to clients; otherwise, <c>false</c>.</value>
        public bool ExposeExceptions { get; set; }

        /// <summary>
        /// Gets the collection of metrics that were generated during the execution of the operation.
        /// </summary>
        /// <value>The metrics.</value>
        public IGraphQueryExecutionMetrics Metrics { get; }

        private bool HasData => this.Data != null;
    }
}