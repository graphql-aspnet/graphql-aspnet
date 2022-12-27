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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Response;

    /// <summary>
    /// Represents a generated response to a query.
    /// </summary>
    [DebuggerDisplay("Messages = {Messages.Count}, Has Data = {HasData}")]
    public class QueryExecutionResult : IQueryExecutionResult
    {
        /// <summary>
        /// Creates a new operation result from a collection of messages. If no messages are included a
        /// general purpose error message is added to the collection.
        /// </summary>
        /// <param name="errorMessages">The set of messages to create a result from.</param>
        /// <param name="queryData">The original, raw query data.</param>
        /// <returns>QueryExecutionResult.</returns>
        public static QueryExecutionResult FromErrorMessages(IGraphMessageCollection errorMessages, GraphQueryData queryData = null)
        {
            Validation.ThrowIfNull(errorMessages, nameof(errorMessages));
            if (errorMessages.Count < 1)
                errorMessages.Critical("An unknown error occured.");

            return new QueryExecutionResult(
                new QueryExecutionRequest(queryData),
                errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutionResult" /> class.
        /// </summary>
        /// <param name="originalRequest">The original request.</param>
        /// <param name="messages">The message collection containing any messages that may
        /// have been generated during execution.</param>
        /// <param name="dataItem">The root resolved data item (The "data" field common to all graphql
        /// query responses).</param>
        /// <param name="metrics">The metrics package that was filled during the operation execution, if any.</param>
        public QueryExecutionResult(
            IQueryExecutionRequest originalRequest,
            IGraphMessageCollection messages = null,
            IQueryResponseFieldSet dataItem = null,
            IQueryExecutionMetrics metrics = null)
        {
            this.QueryRequest = originalRequest;
            this.Data = dataItem;
            this.Messages = new GraphMessageCollection(messages?.Count ?? 0);
            if (messages != null)
                this.Messages.AddRange(messages);

            this.Metrics = metrics;
        }

        /// <summary>
        /// Gets the original operation request that was executed to produce this result.
        /// </summary>
        /// <value>The request.</value>
        public IQueryExecutionRequest QueryRequest { get; }

        /// <summary>
        /// Gets the resultant data item that was generated as a result of completing the operation.
        /// </summary>
        /// <value>The data item that was generated.</value>
        public IQueryResponseFieldSet Data { get; }

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
        public IQueryExecutionMetrics Metrics { get; }

        private bool HasData => this.Data != null;
    }
}