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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

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
            var directivePart = context.FindContextItem<IDirectiveDocumentPart>();
            if (directivePart == null || directivePart.Directive == null || directivePart.Directive.Kind != TypeKind.DIRECTIVE)
            {
                this.ValidationError(
                    context,
                    directivePart.Node,
                    $"The target schema does not contain a directive named '{directivePart.Node.DirectiveName.ToString()}'.");

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