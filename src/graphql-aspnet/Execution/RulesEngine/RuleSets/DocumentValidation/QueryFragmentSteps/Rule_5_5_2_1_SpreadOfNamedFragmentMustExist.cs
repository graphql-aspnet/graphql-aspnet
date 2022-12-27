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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Checks to ensure that when a spread of a named fragment occurs that the named
    /// fragment it targets exists in the document.
    /// </summary>
    internal class Rule_5_5_2_1_SpreadOfNamedFragmentMustExist
        : DocumentPartValidationRuleStep<IFragmentSpreadDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IFragmentSpreadDocumentPart)context.ActivePart;

            // fragment reference is set during part linking during document construction
            if (docPart.Fragment == null)
            {
                this.ValidationError(
                    context,
                    $"The named fragment '{docPart.FragmentName}' does not exist in the supplied document.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.5.2.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragment-spread-target-defined";
    }
}