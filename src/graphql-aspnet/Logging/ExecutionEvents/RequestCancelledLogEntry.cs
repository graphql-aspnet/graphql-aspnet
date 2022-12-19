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
    using System;
    using GraphQL.AspNet.Execution.Contexts;

    /// <summary>
    /// Recorded by an executor after a cancellation request by an external actor was successfully
    /// processed by the runtime.
    /// </summary>
    public class RequestCancelledLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCancelledLogEntry" /> class.
        /// </summary>
        /// <param name="context">The primary query context.</param>
        public RequestCancelledLogEntry(GraphQueryExecutionContext context)
            : base(LogEventIds.RequestCancelled)
        {
            this.OperationRequestId = context?.OperationRequest?.Id.ToString();

            this.TotalExecutionMs = 0;
            if (context?.OperationRequest?.StartTimeUTC != null)
            {
                this.TotalExecutionMs = DateTimeOffset
                    .UtcNow
                    .Subtract(context.OperationRequest.StartTimeUTC)
                    .TotalMilliseconds;
            }
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
        /// Gets the total amount of time the operation executed for.
        /// </summary>
        /// <value>The total execution time in ms.</value>
        public double TotalExecutionMs
        {
            get => this.GetProperty<double>(LogPropertyNames.TOTAL_EXECUTION_TIME);
            private set => this.SetProperty(LogPropertyNames.TOTAL_EXECUTION_TIME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.OperationRequestId?.Length > 8 ? this.OperationRequestId.Substring(0, 8) : this.OperationRequestId;
            return $"Request Cancelled | Id: {idTruncated}";
        }
    }
}