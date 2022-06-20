// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.ValidationRules.RuleSets.DirectiveExecution.DirectiveValidation
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.DirectiveExecution.Common;

    /// <summary>
    /// Checks that the context contains all the expected items
    /// so that it can be processed.
    /// </summary>
    internal class Rule_5_7_1_DirectivesMustExist : DirectiveValidationRuleStep
    {
        /// <inheritdoc />
        public override bool Execute(GraphDirectiveExecutionContext context)
        {
            var contentsAreValid = true;
            if (context?.Directive == null || context.Directive.Kind != TypeKind.DIRECTIVE)
            {
                this.ValidationError(
                    context,
                    "Invalid directive request. No target " +
                    "directive was found for the target data " +
                    $"(data type: {context.Request?.DirectiveTarget?.GetType().FriendlyName()}).");

                contentsAreValid = false;
            }

            return contentsAreValid;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.7.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Directives-Are-Defined";
    }
}