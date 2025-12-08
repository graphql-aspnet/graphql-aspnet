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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>(5.4.3) All required input arguments for all fields/directives must be supplied on the document or declare a default
    /// value in the target schema.</para>
    /// <para>Reference: <see href="https://spec.graphql.org/September2025/#sec-Required-Arguments" /> .</para>
    /// </summary>
    internal class Rule_5_4_3_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnField
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

                if (argument.IsRequired &&
                    !suppliedArguments.ContainsKey(argument.Name.AsMemory()))
                {
                    this.ValidationError(
                        context,
                        fieldSelection.SourceLocation,
                        $"Missing Input Argument. The field '{fieldSelection.Field.SchemaCoordinate}' requires an input argument named '{argument.SchemaCoordinate}'");
                    allArgsValid = false;
                }
            }

            return allArgsValid;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.4.3";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Required-Arguments";
    }
}