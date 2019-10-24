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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A default, concrete implementation of a <see cref="IGraphMessage"/> used
    /// for most generated messages during the execution of a query.
    /// </summary>
    [DebuggerDisplay("{Code} (Severity: {Severity})")]
    public class GraphExecutionMessage : IGraphMessage
    {
        /// <summary>
        /// Creates a graph message from the given rule and message specifics in a common manner.
        /// </summary>
        /// <param name="validationRule">The validation rule that is being broken.</param>
        /// <param name="messageText">The context sensitive message text.</param>
        /// <param name="messageOrigin">The origin location where this message is generated, if any.</param>
        /// <param name="exception">The exception generated with extended details, if any.</param>
        /// <returns>IGraphMessage.</returns>
        internal static IGraphMessage FromValidationRule(
            IValidationRule validationRule,
            string messageText,
            SourceOrigin messageOrigin = null,
            Exception exception = null)
        {
            var graphMessage = new GraphExecutionMessage(
                GraphMessageSeverity.Critical,
                messageText,
                validationRule.ErrorCode,
                messageOrigin,
                exception);
            graphMessage.MetaData.Add("Rule", validationRule.RuleNumber);
            graphMessage.MetaData.Add("RuleReference", validationRule.ReferenceUrl);
            return graphMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphExecutionMessage" /> class.
        /// </summary>
        /// <param name="severity">The severity level of this message.</param>
        /// <param name="message">The message text body.</param>
        /// <param name="code">The code assigned to this message, if any.</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exception">The underlying exception that was thrown by the runtime, if any.</param>
        public GraphExecutionMessage(
            GraphMessageSeverity severity,
            string message,
            string code = null,
            SourceOrigin origin = null,
            Exception exception = null)
        {
            this.Origin = origin ?? SourceOrigin.None;
            this.Code = code?.Trim() ?? "-unknown-";
            this.Message = message?.Trim();
            this.Severity = severity;
            this.Exception = exception;
            this.TimeStamp = DateTimeOffset.UtcNow;
            this.MetaData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the time stamp when this message was created.
        /// </summary>
        /// <value>The time stamp.</value>
        public DateTimeOffset TimeStamp { get; }

        /// <summary>
        /// Gets the origin in the provided source text, if any, this message relates to.
        /// This value is returned as part of a query response.
        /// </summary>
        /// <value>The location.</value>
        public SourceOrigin Origin { get; }

        /// <summary>
        /// Gets an error code identifying this error. This value is returned as part of a query response.
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; }

        /// <summary>
        /// Gets a human-friendly message that conveys details about the error tht occured. This value is
        /// returned as part of a query response.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }

        /// <summary>
        /// Gets an (optional) exception that may have occured to generate the error. The exception
        /// is only conveyed to the requestor if the request is configured to expose exceptions.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the severity of this message that was generated.
        /// </summary>
        /// <value>The severity.</value>
        public GraphMessageSeverity Severity { get; }

        /// <summary>
        /// Gets additional metadata defined for this message. This data will be added as key/value pairs
        /// when the message is rendered to an graph output.
        /// </summary>
        /// <value>The meta data.</value>
        public IDictionary<string, object> MetaData { get; }
    }
}