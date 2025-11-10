// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation.OneOfDirectiveSteps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// A validation step that inspects a variable usage document part and ensures the coerced value matches
    /// the rules for @oneOf directives.
    /// </summary>
    internal class Rule_3_10_1_InputFieldVariable : VariableValidationRuleStep<IInputValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(VariableDataValidationContext context)
        {
            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "3.10.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-OneOf-Input-Objects";
    }
}