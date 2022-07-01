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
    internal class Rule_5_8_2_B_VariablesMustDeclareAValidGraphType
        : DocumentPartValidationRuleStep<IVariableDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && ((IVariableDocumentPart)context.ActivePart).TypeExpression != null; // passed 5.8.2A
        }
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            var variable = (IVariableDocumentPart)context.ActivePart;

            var graphType = variable.GraphType;
            if (graphType == null)
            {
                this.ValidationError(
                    context,
                    $"Unknown Variable Graph Type. The variable named '{variable?.Name}' declares " +
                    $"itself as a graph type of '{variable.TypeExpression}' but the target graph type does not " +
                    "exist in the schema.");

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