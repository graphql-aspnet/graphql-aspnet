// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.OneOfDirectiveSteps
{
    using System.Linq;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Ensures that if a field on any INPUT OBJECT graph type is of a graph type that implements @oneOf, that the field value supplied
    /// conforms to the @oneOf conversion rules
    /// </summary>
    internal class Rule_3_10_1_InputFields : DocumentPartValidationRuleStep<IInputValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            if (!base.ShouldExecute(context))
                return false;

            // only invoke if the input argument's graphtype defines @oneOf
            var arg = (IInputValueDocumentPart)context.ActivePart;
            return arg.GraphType?.AppliedDirectives is not null
                   && arg.GraphType.AppliedDirectives.Includes<OneOfDirective>();
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var inputValueReference = (IInputValueDocumentPart)context.ActivePart;

            if (inputValueReference.TypeExpression.IsListOfItems)
                return this.ValidateListOfInputUnions(context, inputValueReference);

            if (inputValueReference.Value is IComplexSuppliedValueDocumentPart complexValue)
                return this.ValidateComplexValue(context, inputValueReference, complexValue);

            return true;
        }

        private bool ValidateListOfInputUnions(DocumentValidationContext context, IInputValueDocumentPart valueReference)
        {
            // value may be supplied as null, which is fine, if it is a list, we need to validate that each member of the list
            // conforms to the @oneOf requirements
            if (valueReference.Value is IListSuppliedValueDocumentPart listOfValues)
            {
                var allListItemsValid = true;
                var indexValue = 0;
                foreach (var value in listOfValues.ListItems)
                {
                    if (value is IComplexSuppliedValueDocumentPart listItemComplexValue)
                        allListItemsValid = this.ValidateComplexValue(context, valueReference, listItemComplexValue, indexValue++) && allListItemsValid;
                }

                return allListItemsValid;
            }

            // could be that the field value accepts a nullable list, which is fine
            return true;
        }

        private bool ValidateComplexValue(
            DocumentValidationContext context,
            IInputValueDocumentPart valueReference,
            IComplexSuppliedValueDocumentPart complexValue,
            int? indexValue = null)
        {
            // for the complex value, it must have exactly one field defined
            var suppliedFields = complexValue.Fields?.ToArray() ?? [];
            if (suppliedFields.Length != 1)
            {
                var indexSuffix = indexValue.HasValue ? $" for index {indexValue.Value}" : string.Empty;
                this.ValidationError(
                    context,
                    valueReference.Value?.SourceLocation ?? valueReference.SourceLocation,
                    $"Invalid input value. The graph type, '{valueReference.GraphType.Name}', is declared as an input union (i.e. '@oneOf') and only one field may " +
                    $"be supplied. Received {suppliedFields.Length} fields{indexSuffix}. ({string.Join(", ", suppliedFields.Select(x => x.Key))})");

                return false;
            }

            // if the value of the single field is an object literal we have to make sure its non-null
            // rule set 5.8.x will validation coercion rules of said value, we dont need to check that here.
            var inputField = suppliedFields[0];
            var inputFieldValue = inputField.Value;

            if (inputFieldValue.Value is INullSuppliedValueDocumentPart)
            {
                var indexSuffix = indexValue.HasValue ? $" for index {indexValue.Value}" : string.Empty;
                this.ValidationError(
                    context,
                    valueReference.Value?.SourceLocation ?? valueReference.SourceLocation,
                    $"Invalid input value. The graph type, '{valueReference.GraphType.Name}', is declared as an input union (i.e. '@oneOf') " +
                    $"and the single supplied field must be non-null. Received field '{inputField.Key}' with a null value{indexSuffix}.");

                return false;
            }

            // Note: If the value of the input field is a variable reference we still have to validate that
            // the value of the variable (at runtime) is non-null. We do that during the variable resolution stage
            // of query processing. This rule only validates what's avialable on the parsed document.
            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "3.10.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-OneOf-Input-Objects";
    }
}