// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that any input objects supply all the required fields of their argument definition in the target schema.
    /// </summary>
    internal class Rule_5_6_4_InputObjectRequiredFieldsMustBeProvided : DocumentPartValidationRuleStep
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is IInputValueDocumentPart ivdp &&
                ivdp.Value is QueryComplexInputValue;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var ivdp = context.ActivePart as IInputValueDocumentPart;
            var graphType = ivdp.GraphType as IInputObjectGraphType;
            var requiredFields = graphType?.Fields.Where(x => x.TypeExpression.IsRequired).ToList();
            var complexValue = ivdp.Value as QueryComplexInputValue;
            if (complexValue == null || requiredFields == null)
            {
                this.ValidationError(
                    context,
                    ivdp.Node,
                    $"Input type mismatch. The {ivdp.InputType} '{ivdp.Name}' was used like an {TypeKind.INPUT_OBJECT.ToString()} " +
                    $"but contained no fields to evaluate. Check the schema definition for {ivdp.TypeExpression.TypeName}.");
                return false;
            }

            var allFieldsAccountedFor = true;
            foreach (var field in requiredFields)
            {
                if (!complexValue.Arguments.ContainsKey(field.Name))
                {
                    this.ValidationError(
                        context,
                        ivdp.Node,
                        $"The {ivdp.InputType} '{ivdp.Name}' requires a field named '{field.Name}'.");
                    allFieldsAccountedFor = false;
                }
            }

            return allFieldsAccountedFor;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.6.4";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Input-Object-Required-Fields";
    }
}