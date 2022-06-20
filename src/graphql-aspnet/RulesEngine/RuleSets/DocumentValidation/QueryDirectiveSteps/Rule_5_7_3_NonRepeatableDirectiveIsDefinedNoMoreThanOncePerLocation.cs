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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Ensures that for the location where this directive exists no other
    /// directive was parsed with the same name.
    /// </summary>
    internal class Rule_5_7_3_NonRepeatableDirectiveIsDefinedNoMoreThanOncePerLocation
        : DocumentPartValidationRuleStep<IDirectiveDocumentPart>
    {

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var thisDirective = context.ActivePart as IDirectiveDocumentPart;
            var container = context.ActivePart.Parent as IDirectiveContainerDocumentPart;

            if (thisDirective == null || container == null)
                return false;

            // skip validation of this rule if this second (or more) encountered
            // instance of this directive in the document
            // we only need to validate it once per directive name
            var firstInstance = container.Directives.FirstOrDefault(x => x.Name == thisDirective.Name);
            if (firstInstance != null && firstInstance != thisDirective)
                return true;

            if (!thisDirective.Directive.IsRepeatable && container.Directives.Count(x =>
                    x != thisDirective
                    && x.Directive.Name == thisDirective.Directive.Name) > 0)
            {
                this.ValidationError(
                    context,
                    thisDirective.Node,
                    $"The directive '{thisDirective.Name}' is already defined in this location in the query document. " +
                    $"Non-repeatable directives must be unique per instantiated location (e.g. once per field, once per input argument etc.).");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.7.3";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Directives-Are-Unique-Per-Location";
    }
}