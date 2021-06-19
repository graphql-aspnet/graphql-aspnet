// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common
{
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    internal abstract class DocumentPartValidationRuleStep : DocumentPartValidationStep, IValidationRule
    {
        /// <summary>
        /// Registers a validation error with the local message collection as a critical error. The validation
        /// message will automatically be appended with the appropriate message extensions to reference the error being validated.
        /// </summary>
        /// <param name="context">The validation context in scope.</param>
        /// <param name="node">The node in scope when the error was generated.</param>
        /// <param name="message">The error message.</param>
        protected void ValidationError(DocumentValidationContext context, SyntaxNode node,  string message)
        {
            var graphMessage = GraphExecutionMessage.FromValidationRule(
                this,
                message,
                node.Location.AsOrigin());

            context.Messages.Add(graphMessage);
        }

        /// <summary>
        /// Gets the error code to associate with the broken rule.
        /// </summary>
        /// <value>The error code.</value>
        public string ErrorCode => Constants.ErrorCodes.INVALID_DOCUMENT;

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public abstract string RuleNumber { get; }

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If <see cref="ReferenceUrl"/> is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected abstract string RuleAnchorTag { get; }

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        public virtual string ReferenceUrl => ReferenceRule.Create(this.RuleAnchorTag);
    }
}