// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that any input objects supply all the required fields of their argument definition in the target schema.
    /// </summary>
    internal class Rule_5_6_4_InputObjectRequiredFieldsMustBeProvided
        : DocumentPartValidationRuleStep<IComplexSuppliedValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && ((IComplexSuppliedValueDocumentPart)context.ActivePart).GraphType is IInputObjectGraphType;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var complexValue = context.ActivePart as IComplexSuppliedValueDocumentPart;
            var argument = complexValue.Parent as IInputArgumentDocumentPart;
            var graphType = complexValue.GraphType as IInputObjectGraphType;
            var requiredFields = graphType?.Fields.RequiredFields;

            if (requiredFields == null)
            {
                this.ValidationError(
                    context,
                    $"Input type mismatch. The input argument '{argument.Name}' was used like an {TypeKind.INPUT_OBJECT.ToString()} " +
                    $"but contained no fields to evaluate. Check the schema definition for {graphType.Name}.");
                return false;
            }

            var allFieldsAccountedFor = true;
            foreach (var field in requiredFields)
            {
                if (!complexValue.TryGetArgument(field.Name, out _))
                {
                    this.ValidationError(
                        context,
                        $"The input graph type '{graphType.Name}' requires a field named '{field.Name}'.");

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