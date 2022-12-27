// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.GeneralEvents
{
    using System;
    using GraphQL.AspNet.Execution.Contexts;

    /// <summary>
    /// Recorded by an executor after a cancellation request by an external actor was successfully
    /// processed by the runtime.
    /// </summary>
    public class RequestTimedOutLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTimedOutLogEntry" /> class.
        /// </summary>
        /// <param name="context">The primary query context.</param>
        public RequestTimedOutLogEntry(QueryExecutionContext context)
            : base(LogEventIds.RequestTimeout)
        {
            var startDate = context?.QueryRequest?.StartTimeUTC ?? DateTimeOffset.MinValue;
            this.QueryRequestId = context?.QueryRequest?.Id.ToString();

            this.TotalExecutionMs = 0;
            if (context?.QueryRequest?.StartTimeUTC != null)
            {
                this.TotalExecutionMs = DateTimeOffset
                    .UtcNow
                    .Subtract(context.QueryRequest.StartTimeUTC)
                    .TotalMilliseconds;
            }
        }

        /// <summary>
        /// Gets the globally unique id of the query request on this event.
        /// </summary>
        /// <value>The unique operation request id.</value>
        public string QueryRequestId
        {
            get => this.GetProperty<string>(LogPropertyNames.QUERY_REQUEST_ID);
            private set => this.SetProperty(LogPropertyNames.QUERY_REQUEST_ID, value);
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
            var idTruncated = this.QueryRequestId?.Length > 8 ? this.QueryRequestId.Substring(0, 8) : this.QueryRequestId;
            return $"Request Timed Out | Id: {idTruncated} | Total Time (ms): {this.TotalExecutionMs}";
        }
    }
}