// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryDirectiveSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A rule that checks a directive node's parent to ensure that the location the directive is attached to
    /// is valid based on the directive definition in the target schema.
    /// </summary>
    internal class Rule_5_7_1_DirectiveMustBeDefinedInTheSchema
        : DocumentPartValidationRuleStep<IDirectiveDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var directivePart = (IDirectiveDocumentPart)context.ActivePart;
            if (directivePart.GraphType == null || directivePart.GraphType.Kind != TypeKind.DIRECTIVE)
            {
                this.ValidationError(
                    context,
                    directivePart.Node,
                    $"The target schema does not contain a directive named '{directivePart.DirectiveName}'.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.7.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Directives-Are-Defined";
    }
}