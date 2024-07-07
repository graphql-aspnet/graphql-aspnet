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
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;

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
            SourceOrigin messageOrigin = default,
            Exception exception = null)
        {
            return FromValidationRule(
                validationRule.RuleNumber,
                validationRule.ReferenceUrl,
                validationRule.ErrorCode,
                messageText,
                messageOrigin,
                exception);
        }

        /// <summary>
        /// Creates a graph message from the given rule and message specifics in a common manner.
        /// </summary>
        /// <param name="ruleNumber">The rule number that failed validation.</param>
        /// <param name="url">The url pointing to the rule in the specification.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="messageText">The context sensitive message text.</param>
        /// <param name="messageOrigin">The origin location where this message is generated, if any.</param>
        /// <param name="exception">The exception generated with extended details, if any.</param>
        /// <returns>IGraphMessage.</returns>
        internal static IGraphMessage FromValidationRule(
            string ruleNumber,
            string url,
            string errorCode,
            string messageText,
            SourceOrigin messageOrigin = default,
            Exception exception = null)
        {
            var graphMessage = new GraphExecutionMessage(
                GraphMessageSeverity.Critical,
                messageText,
                errorCode,
                messageOrigin,
                exception);

            graphMessage.MetaData.Add(Constants.Messaging.REFERENCE_RULE_NUMBER_KEY, ruleNumber);
            graphMessage.MetaData.Add(Constants.Messaging.REFERENCE_RULE_URL_KEY, url);
            return graphMessage;
        }

        private string _message;
        private string _code;

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
            SourceOrigin origin = default,
            Exception exception = null)
        {
            this.Origin = origin;
            this.Code = code;
            this.Message = message;
            this.Severity = severity;
            this.Exception = exception;
            this.TimeStamp = DateTimeOffset.UtcNow;
            this.MetaData = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public DateTimeOffset TimeStamp { get; }

        /// <inheritdoc />
        public SourceOrigin Origin { get; }

        /// <inheritdoc />
        public string Code
        {
            get => _code;
            set => _code = value?.Trim() ?? Constants.ErrorCodes.DEFAULT;
        }

        /// <inheritdoc />
        public string Message
        {
            get => _message;
            set => _message = value?.Trim();
        }

        /// <inheritdoc />
        public Exception Exception { get; set; }

        /// <inheritdoc />
        public GraphMessageSeverity Severity { get; }

        /// <inheritdoc />
        public IDictionary<string, object> MetaData { get; }
    }
}