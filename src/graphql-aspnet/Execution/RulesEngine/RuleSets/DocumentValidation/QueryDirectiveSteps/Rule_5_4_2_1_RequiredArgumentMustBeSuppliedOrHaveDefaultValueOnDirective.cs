// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryDirectiveSteps
{
    using System;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// All required input arguments for all fields/directives must be supplied on the document or declare a default
    /// value in the target schema.
    /// </summary>
    internal class Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnDirective
        : DocumentPartValidationRuleStep<IDirectiveDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IDirectiveDocumentPart)context.ActivePart;
            var directiveIsValid = true;

            var directive = docPart.GraphType as IDirective;
            if (directive != null)
            {
                var suppliedArgs = docPart.Arguments;

                // inspect all declared arguments from the schema
                foreach (var argument in directive.Arguments)
                {
                    if (argument.IsRequired && !suppliedArgs.ContainsKey(argument.Name.AsMemory()))
                    {
                        this.ValidationError(
                            context,
                            docPart.SourceLocation,
                            $"Missing Input Argument. The directive '{docPart.DirectiveName}' requires an input argument named '{argument.Name}'");
                        directiveIsValid = false;
                    }
                }
            }

            return directiveIsValid;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.4.2.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Required-Arguments";
    }
}