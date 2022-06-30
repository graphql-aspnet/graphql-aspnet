// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.VariableNodeSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule that dictates each variables core type must be a SCALAR, ENUM or INPUT_OBJECT.
    /// </summary>
    internal class Rule_5_8_2_A_VariablesMustDeclareAType
        : DocumentPartValidationRuleStep<IVariableDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var variable = (IVariableDocumentPart)context.ActivePart;
            if (variable.TypeExpression == null || !variable.TypeExpression.IsValid)
            {
                this.ValidationError(
                    context,
                    "Unknown Graph Type. Could not determine the graph type expression of the variable " +
                    $"named '{variable.Name}'. Double check that your variable declaration is correct.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.8.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Variables-Are-Input-Types";
    }
}