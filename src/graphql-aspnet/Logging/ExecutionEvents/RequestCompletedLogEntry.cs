// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// Recorded by an executor after the entire graphql operation has been completed
    /// and final results have been generated.
    /// </summary>
    public class RequestCompletedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCompletedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The primary query context.</param>
        public RequestCompletedLogEntry(GraphQueryExecutionContext context)
            : base(LogEventIds.RequestCompleted)
        {
            this.OperationRequestId = context.OperationRequest.Id;
            this.ResultHasErrors = context.Messages.Severity.IsCritical();
            this.ResultHasData = context.Result?.Data != null;
        }

        /// <summary>
        /// Gets the globally unique id of the operation request on this event.
        /// </summary>
        /// <value>The unique operation request id.</value>
        public string OperationRequestId
        {
            get => this.GetProperty<string>(LogPropertyNames.OPERATION_REQUEST_ID);
            private set => this.SetProperty(LogPropertyNames.OPERATION_REQUEST_ID, value);
        }

        /// <summary>
        /// Gets a value indicating whether the completed result being sent to the requestor
        /// contains some errors.
        /// </summary>
        /// <value><c>true</c> if operation result contains errors; otherwise, <c>false</c>.</value>
        public bool ResultHasErrors
        {
            get => this.GetProperty<bool>(LogPropertyNames.OPERATION_RESULT_HAS_ERRORS);
            private set => this.SetProperty(LogPropertyNames.OPERATION_RESULT_HAS_ERRORS, value);
        }

        /// <summary>
        /// Gets a value indicating whether the completed result being sent to the requestor
        /// contains a data segment.
        /// </summary>
        /// <value><c>true</c> if operation result contains some data; otherwise, <c>false</c>.</value>
        public bool ResultHasData
        {
            get => this.GetProperty<bool>(LogPropertyNames.OPERATION_RESULT_HAS_DATA);
            private set => this.SetProperty(LogPropertyNames.OPERATION_RESULT_HAS_DATA, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.OperationRequestId?.Length > 8 ? this.OperationRequestId.Substring(0, 8) : this.OperationRequestId;
            return $"Request Completed | Id: {idTruncated}, Has Data: {this.ResultHasData}, Has Errors: {this.ResultHasErrors}";
        }
    }
}