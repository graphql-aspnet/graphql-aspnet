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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule that dictates each variables core type must be a SCALAR, ENUM or INPUT_OBJECT.
    /// </summary>
    internal class Rule_5_8_2_C_VariableGraphTypeMustBeOfAllowedTypeKinds
        : DocumentPartValidationRuleStep<IVariableDocumentPart>
    {

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && ((IVariableDocumentPart)context.ActivePart).GraphType != null
                && ((IVariableDocumentPart)context.ActivePart).TypeExpression != null;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var variable = (IVariableDocumentPart)context.ActivePart;
            var kind = variable.GraphType.Kind;
            if (!kind.IsValidInputKind())
            {
                this.ValidationError(
                    context,
                    $"Invalid Variable Graph Type. The variable named '${variable.Name}' references the graph type " +
                    $"'{variable.GraphType.Name}' which is of kind {variable.GraphType.Kind}.  Only " +
                    $"{TypeKind.SCALAR}, {TypeKind.ENUM} and '{TypeKind.INPUT_OBJECT}' are allowed for " +
                    "variable declarations.");

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