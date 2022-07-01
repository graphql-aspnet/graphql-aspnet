// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryVariableSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    internal class Rule_5_8_3_AllVariableUsagesDefined
        : DocumentPartValidationRuleStep<IVariableUsageDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {

        }

        /// <inheritdoc />
        public override string RuleNumber => "5.8.3";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-All-Variable-Uses-Defined";
    }
}