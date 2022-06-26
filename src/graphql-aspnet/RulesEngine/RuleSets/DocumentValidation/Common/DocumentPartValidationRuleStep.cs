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
        /// <inheritdoc />
        protected override void ValidationError(DocumentValidationContext context, SyntaxNode node, string message)
        {
            var graphMessage = GraphExecutionMessage.FromValidationRule(
             this,
             message,
             node.Location.AsOrigin());

            context.Messages.Add(graphMessage);
        }

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