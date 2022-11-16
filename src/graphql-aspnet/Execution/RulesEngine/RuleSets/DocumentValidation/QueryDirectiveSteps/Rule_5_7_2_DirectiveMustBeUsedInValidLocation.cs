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
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that the item the directive is acting on/against is valid for the directive as its defined in the target schema.
    /// </summary>
    internal class Rule_5_7_2_DirectiveMustBeUsedInValidLocation
        : DocumentPartValidationRuleStep<IDirectiveDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context) && ((IDirectiveDocumentPart)context.ActivePart).GraphType is IDirective;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IDirectiveDocumentPart)context.ActivePart;
            var directive = (IDirective)docPart.GraphType;
            if (docPart == null || directive == null)
                return false;

            if (!directive.Locations.HasFlag(docPart.Location))
            {
                var locationName = docPart.Location.ToString();
                var allowedLocations = directive.Locations.GetIndividualFlags<DirectiveLocation>();

                var allowedString = string.Join(", ", allowedLocations.Select(x => x.ToString()));
                this.ValidationError(
                    context,
                    $"Invalid directive location. Attempted use of '{directive.Name}' at location '{locationName}'. " +
                    $"Allowed Locations:  {allowedString}");
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.7.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Directives-Are-In-Valid-Locations";
    }
}