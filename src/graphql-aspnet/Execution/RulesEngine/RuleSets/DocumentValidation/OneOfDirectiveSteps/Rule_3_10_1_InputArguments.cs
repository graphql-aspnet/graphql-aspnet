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
    /// Ensures that for an object literal supplied to an input object argument, when that object type declares the
    /// '@oneOf' directive, that only one field value was supplied on the document and that it was non-null.
    /// </summary>
    internal class Rule_3_10_1_InputArguments : DocumentPartValidationRuleStep<IInputArgumentDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            if (!base.ShouldExecute(context))
                return false;

            // only invoke if the input argument's graphtype defines @oneOf
            var arg = (IInputArgumentDocumentPart)context.ActivePart;
            return arg.GraphType?.AppliedDirectives is not null
                   && arg.GraphType.AppliedDirectives.Includes<OneOfDirective>();
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var argument = (IInputArgumentDocumentPart)context.ActivePart;

            if (argument.TypeExpression.IsListOfItems)
                return this.ValidateListOfInputUnions(context, argument);

            if (argument.Value is IComplexSuppliedValueDocumentPart complexValue)
                return this.ValidateComplexValue(context, argument, complexValue);

            return true;
        }

        private bool ValidateListOfInputUnions(DocumentValidationContext context, IInputArgumentDocumentPart argument)
        {
            // value may be supplied as null, which is fine, if it is a list, we need to validate that each member of the list
            // conforms to the @oneOf requirements
            if (argument.Value is IListSuppliedValueDocumentPart listOfValues)
            {
                var allListItemsValid = true;
                var indexValue = 0;
                foreach (var value in listOfValues.ListItems)
                {
                    if (value is IComplexSuppliedValueDocumentPart listItemComplexValue)
                        allListItemsValid = this.ValidateComplexValue(context, argument, listItemComplexValue, indexValue++) && allListItemsValid;
                }

                return allListItemsValid;
            }

            // could be that the argument accepts a nullable list, which is fine
            return true;
        }

        private bool ValidateComplexValue(
            DocumentValidationContext context,
            IInputArgumentDocumentPart argument,
            IComplexSuppliedValueDocumentPart complexValue,
            int? indexValue = null)
        {
            // for the complex, it must have exactly one field defined
            var suppliedFields = complexValue.Fields?.ToArray() ?? [];
            if (suppliedFields.Length != 1)
            {
                var indexSuffix = indexValue.HasValue ? $" for index {indexValue.Value}" : string.Empty;
                this.ValidationError(
                    context,
                    argument.Value?.SourceLocation ?? argument.SourceLocation,
                    $"Invalid input argument. The value(s) for the input argument named '{argument.Name}' could " +
                    $"not be coerced correctly. The graph type, '{argument.GraphType.Name}', is declared as an input union (i.e. '@oneOf') and only one field may " +
                    $"be supplied. Received {suppliedFields.Length} fields{indexSuffix}. ({string.Join(", ", suppliedFields.Select(x => x.Key))})");

                return false;
            }

            // the value supplied to the singular field must be non-null
            var inputField = suppliedFields[0];
            var inputFieldValue = inputField.Value;

            // if the value of the single field is an object literal we have to make sure its non-null
            // rule set 5.8.x will validation coercion rules of said value, we dont need to check that here.
            if (inputFieldValue.Value is INullSuppliedValueDocumentPart)
            {
                this.ValidationError(
                    context,
                    argument.Value?.SourceLocation ?? argument.SourceLocation,
                    $"Invalid input argument. The value for the input argument named '{argument.Name}' could " +
                    $"not be coerced correctly. The type is declared as an input union (i.e. '@oneOf') and the single supplied " +
                    $"field must be non-null. Received field '{inputField.Key}' with a null value.");

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