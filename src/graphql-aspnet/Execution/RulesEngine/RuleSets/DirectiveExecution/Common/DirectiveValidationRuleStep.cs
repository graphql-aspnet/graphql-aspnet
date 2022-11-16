﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DirectiveExecution.Common
{
    using System;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.Interfaces;

    /// <summary>
    /// A base step for any rules targeting the validation of
    /// a request to invoke a directive.
    /// </summary>
    internal abstract class DirectiveValidationRuleStep : DirectiveValidationStep, IValidationRule
    {
        /// <summary>
        /// Registers a validation error with the local message collection as a critical error. The validation
        /// message will automatically be appended with the appropriate message extensions to reference the error being validated.
        /// </summary>
        /// <param name="context">The validation context in scope.</param>
        /// <param name="message">The error message to apply.</param>
        /// <param name="exception">The exception to add to the message, if any.</param>
        protected void ValidationError(
            GraphDirectiveExecutionContext context,
            string message,
            Exception exception = null)
        {
            var graphMessage = GraphExecutionMessage.FromValidationRule(
                this,
                message,
                context.Request.Origin,
                exception);

            context.Messages.Add(graphMessage);
        }

        /// <inheritdoc />
        public virtual string ErrorCode => Constants.ErrorCodes.EXECUTION_ERROR;

        /// <inheritdoc />
        public abstract string RuleNumber { get; }

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If <see cref="ReferenceUrl"/> is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected abstract string RuleAnchorTag { get; }

        /// <inheritdoc />
        public virtual string ReferenceUrl => ReferenceRule.CreateFromAnchorTag(this.RuleAnchorTag);
    }
}