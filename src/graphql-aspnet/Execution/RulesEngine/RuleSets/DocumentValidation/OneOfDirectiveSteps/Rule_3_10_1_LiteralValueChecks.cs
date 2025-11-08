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
    internal class Rule_3_10_1_LiteralValueChecks : DocumentPartValidationRuleStep<IInputValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            // does the target graph type implement @oneOf? if not, skip validation, the rules don't apply
            return base.ShouldExecute(context)
                   && context.ActivePart is IInputArgumentDocumentPart { Value: IComplexSuppliedValueDocumentPart } argument
                   && argument.GraphType.AppliedDirectives.Includes<OneOfDirective>();
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var argument = context.ActivePart as IInputArgumentDocumentPart;

            var didEvaluate = this.TryValidateFieldCount(argument, out var isValid, out var suppliedFields);
            if (didEvaluate && !isValid)
            {
                this.ValidationError(
                    context,
                    argument.Value?.SourceLocation ?? argument.SourceLocation,
                    $"Invalid input argument. The value for the input argument named '{argument.Name}' could " +
                    $"not be coerced correctly. The type is declared as an input union (i.e. '@oneOf') and only one field may " +
                    $"be supplied. Received {suppliedFields.Length} fields. ({string.Join(", ", suppliedFields)})");

                return false;
            }

            didEvaluate = this.TryValidateNonNullFieldValue(argument, out isValid, out var fieldName);
            if (didEvaluate && !isValid)
            {
                this.ValidationError(
                    context,
                    argument.Value?.SourceLocation ?? argument.SourceLocation,
                    $"Invalid input argument. The value for the input argument named '{argument.Name}' could " +
                    $"not be coerced correctly. The type is declared as an input union (i.e. '@oneOf') and the single supplied " +
                    $"field must be non-null. Received field '{fieldName}' with a null value.");

                return false;
            }

            return true;
        }

        private bool TryValidateFieldCount(IInputArgumentDocumentPart argument, out bool isValid, out string[] fieldsSupplied)
        {
            fieldsSupplied = [];
            isValid = false;

            // always true based on the ShouldExecute criteria
            var complexValue = argument.Value as IComplexSuppliedValueDocumentPart;

            // 0 supplied fields is invalid
            if (complexValue.Fields is null || complexValue.Fields.Count == 0)
                return true;

            fieldsSupplied = complexValue.Fields.Select(x => x.Key).ToArray();
            if (complexValue.Fields.Count > 1)
                return true;

            isValid = true;
            return true;
        }

        private bool TryValidateNonNullFieldValue(IInputArgumentDocumentPart argument, out bool isValid, out string fieldName)
        {
            fieldName = null;
            isValid = false;

            if (argument.Value is not IComplexSuppliedValueDocumentPart complexValue)
                return false;

            // grab a reference to the value that was supplied in the document
            var field = complexValue.Fields.First();
            fieldName = field.Key;

            var valuePart = field.Value;
            var suppliedValue = valuePart.Value;

            // if the value for the field is a variable reference
            // we can't validate it yet
            if (suppliedValue is IVariableUsageDocumentPart)
                return false;

            // valiate that the field value isnt 'null'
            if (suppliedValue is INullSuppliedValueDocumentPart)
                return true;

            isValid = true;
            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "3.10.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-OneOf-Input-Objects";
    }
}