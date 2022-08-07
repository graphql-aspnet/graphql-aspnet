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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Ensures that for the location where this directive exists no other
    /// directive was parsed with the same name.
    /// </summary>
    internal class Rule_5_7_3_NonRepeatableDirectiveIsDefinedNoMoreThanOncePerLocation
        : DocumentPartValidationRuleStep<IDirectiveDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            if (!base.ShouldExecute(context))
                return false;

            // skip this rule is repeatability is allowed for the target directive
            if (((IDirectiveDocumentPart)context.ActivePart).GraphType is IDirective dir)
            {
                return !dir.IsRepeatable;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IDirectiveDocumentPart)context.ActivePart;
            var directive = (IDirective)docPart.GraphType;

            if (docPart == null || context.ParentPart == null)
                return false;

            // skip validation of this rule if this second (or more) encountered
            // instance of this directive in the document
            // we only need to validate its duplication once per directive name
            var firstInstance = context.ParentPart.Children[DocumentPartType.Directive]
                .OfType<IDirectiveDocumentPart>()
                .FirstOrDefault(x => x.DirectiveName == docPart.DirectiveName);

            if (firstInstance != null && firstInstance != docPart)
                return true;

            var totalWithName = context.ParentPart.Children[DocumentPartType.Directive]
                .OfType<IDirectiveDocumentPart>().Count(x => x.DirectiveName == docPart.DirectiveName);

            if (totalWithName > 1)
            {
                this.ValidationError(
                    context,
                    docPart.Node,
                    $"The directive '{docPart.DirectiveName}' is already defined in this location in the query document. " +
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