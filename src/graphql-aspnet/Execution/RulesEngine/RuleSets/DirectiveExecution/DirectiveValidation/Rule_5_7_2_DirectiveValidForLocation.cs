// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DirectiveExecution.DirectiveValidation
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DirectiveExecution.Common;

    /// <summary>
    /// A rule to ensure that the location where the directive is being
    /// executed is valid for the target directive.
    /// </summary>
    internal class Rule_5_7_2_DirectiveValidForLocation : DirectiveValidationRuleStep
    {
        /// <inheritdoc />
        public override bool Execute(GraphDirectiveExecutionContext context)
        {
            var targetLocation = context.Request.InvocationContext.Location;
            var isValidLocation = (targetLocation & context.Directive.Locations) > 0;

            if (!isValidLocation)
            {
                this.ValidationError(
                    context,
                    $"Invalid Directive Location. The target location '{targetLocation}' " +
                    $"is not supported by the directive '{context.Directive.Name}'");
            }

            return isValidLocation;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.7.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Directives-Are-In-Valid-Locations";
    }
}