// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Ensures that any declared fragment on the document context is used/spread at least once
    /// in the supplied operations.
    /// </summary>
    internal class Rule_5_5_1_4_AllDeclaredFragmentsMustBeUsed : DocumentPartValidationRuleStep<QueryFragment>
    {
        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            var fragment = (QueryFragment)context.ActivePart;
            if (!fragment.IsReferenced)
            {
                this.ValidationError(
                    context,
                    fragment.Node,
                    $"The named fragment '{fragment.Name}' was not referenced by an operation. " +
                    "All declared fragments must be used at least once.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.1.4";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Fragments-Must-Be-Used";
    }
}