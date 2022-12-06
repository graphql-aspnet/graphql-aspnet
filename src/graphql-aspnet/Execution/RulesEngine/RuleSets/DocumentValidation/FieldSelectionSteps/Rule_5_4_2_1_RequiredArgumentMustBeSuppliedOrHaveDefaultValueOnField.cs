// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSteps
{
    using System;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// All required input arguments for all fields/directives must be supplied on the document or declare a default
    /// value in the target schema.
    /// </summary>
    internal class Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnField
        : DocumentPartValidationRuleStep<IFieldDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var fieldSelection = (IFieldDocumentPart)context.ActivePart;

            // inspect all declared arguments from the schema
            var allArgsValid = true;
            var suppliedArguments = fieldSelection.Arguments;
            foreach (var argument in fieldSelection.Field.Arguments)
            {
                // any argument flaged as being a source input (such as for type extensions)
                // or internal (such as subscription event sources)
                // and can be skipped when validating query document
                if (argument.ArgumentModifiers.IsSourceParameter())
                    continue;
                if (argument.ArgumentModifiers.IsInternalParameter())
                    continue;

                // when the argument is required but the schema defines no value
                // and it was not on the user query document this rule fails
                if (argument.TypeExpression.IsNonNullable &&
                    argument.DefaultValue == null &&
                    !suppliedArguments.ContainsKey(argument.Name.AsMemory()))
                {
                    this.ValidationError(
                        context,
                        fieldSelection.SourceLocation,
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