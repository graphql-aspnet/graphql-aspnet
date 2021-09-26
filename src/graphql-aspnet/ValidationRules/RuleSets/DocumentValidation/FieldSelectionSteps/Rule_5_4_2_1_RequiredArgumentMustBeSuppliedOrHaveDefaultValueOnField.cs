// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.FieldSelectionSteps
{
    using System;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// All required input arguments for all fields/directives must be supplied on the document or declare a default
    /// value in the target schema.
    /// </summary>
    internal class Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnField : DocumentPartValidationRuleStep<FieldSelection>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var fieldSelection = (FieldSelection)context.ActivePart;

            // inspect all declared arguments from the schema
            var allArgsValid = true;
            foreach (var argument in fieldSelection.Field.Arguments)
            {
                // when the argument is required but the schema defines no value
                // and it was not on the user query document this rule fails
                if (argument.TypeExpression.IsRequired &&
                    argument.DefaultValue == null &&
                    !fieldSelection.Arguments.ContainsKey(argument.Name.AsMemory()))
                {
                    this.ValidationError(
                        context,
                        fieldSelection.Node,
                        $"Missing Input Argument. The field '{fieldSelection.Name}' requires an input argument named '{argument.Name}'");
                    allArgsValid = false;
                }
            }

            return allArgsValid;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.4.2.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Required-Arguments";
    }
}