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
        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.4.2.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Required-Arguments";
    }
}