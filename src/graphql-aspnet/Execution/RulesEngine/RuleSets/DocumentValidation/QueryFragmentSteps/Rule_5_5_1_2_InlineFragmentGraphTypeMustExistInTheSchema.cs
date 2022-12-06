// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// An inlined fragmented spread's target type must exist within the target schema.
    /// </summary>
    internal class Rule_5_5_1_2_InlineFragmentGraphTypeMustExistInTheSchema
        : DocumentPartValidationRuleStep<IFragmentDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var fragment = (IFragmentDocumentPart)context.ActivePart;

            if (fragment.GraphType == null)
            {
                this.ValidationError(
                    context,
                    $"No known graph type was found for the target fragment.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.5.1.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragment-Spread-Type-Existence";
    }
}