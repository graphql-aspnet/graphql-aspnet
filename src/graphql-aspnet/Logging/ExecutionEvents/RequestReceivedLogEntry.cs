﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when a new request is generated by a query controller and passed to an
    /// executor for processing. This event is recorded before any action is taken.
    /// </summary>
    public class RequestReceivedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestReceivedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RequestReceivedLogEntry(GraphQueryExecutionContext context)
            : base(LogEventIds.RequestReceived)
        {
            this.OperationRequestId = context?.OperationRequest?.Id.ToString();
            this.Username = context?.SecurityContext?.DefaultUser?.Identity?.Name;
            this.QueryOperationName = context?.OperationRequest?.OperationName;
            this.QueryText = context?.OperationRequest?.QueryText;
        }

        /// <summary>
        /// Gets the username of the user on the request, if any.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get => this.GetProperty<string>(LogPropertyNames.USERNAME);
            private set => this.SetProperty(LogPropertyNames.USERNAME, value);
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
        /// Gets the query text submitted with the request.
        /// </summary>
        /// <value>The query text.</value>
        public string QueryText
        {
            get => this.GetProperty<string>(LogPropertyNames.QUERY_TEXT);
            private set => this.SetProperty(LogPropertyNames.QUERY_TEXT, value);
        }

        /// <summary>
        /// Gets the named operation (of the query text) to execute. May be null.
        /// </summary>
        /// <value>The operation name, defined in the query text, to execute.</value>
        public string QueryOperationName
        {
            get => this.GetProperty<string>(LogPropertyNames.QUERY_OPERATION_NAME);
            private set => this.SetProperty(LogPropertyNames.QUERY_OPERATION_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.OperationRequestId?.Length > 8 ? this.OperationRequestId.Substring(0, 8) : this.OperationRequestId;
            return $"Graph Query Received | User: {this.Username ?? "{anon}"},  Id: {idTruncated}, Query Length: {this.QueryText.Length}";
        }
    }
}