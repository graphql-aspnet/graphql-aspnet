// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Checks to ensure that when a spread of a named fragment occurs (<see cref="FragmentSpreadNode"/>) that the named
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