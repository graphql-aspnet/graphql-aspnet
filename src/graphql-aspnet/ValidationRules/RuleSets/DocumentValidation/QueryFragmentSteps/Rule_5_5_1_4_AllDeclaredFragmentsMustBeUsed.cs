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
        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string RuleNumber => "5.5.1.4";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragments-Must-Be-Used";
    }
}