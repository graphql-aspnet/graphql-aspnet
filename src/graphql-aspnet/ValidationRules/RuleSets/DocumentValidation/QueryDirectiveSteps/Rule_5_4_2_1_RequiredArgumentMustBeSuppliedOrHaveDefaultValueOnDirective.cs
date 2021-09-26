// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryDirectiveSteps
{
    using System;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// All required input arguments for all fields/directives must be supplied on the document or declare a default
    /// value in the target schema.
    /// </summary>
    internal class Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnDirective : DocumentPartValidationRuleStep<QueryDirective>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var queryDirective = (QueryDirective)context.ActivePart;
            var directiveIsValid = true;

            // inspect all declared arguments from the schema
            foreach (var argument in queryDirective.Directive.Arguments)
            {
                if (argument.DefaultValue == null && !queryDirective.Arguments.ContainsKey(argument.Name.AsMemory()))
                {
                    this.ValidationError(
                        context,
                        queryDirective.Node,
                        $"Missing Input Argument. The directive '{queryDirective.Name}' requires an input argument named '{argument.Name}'");
                    directiveIsValid = false;
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