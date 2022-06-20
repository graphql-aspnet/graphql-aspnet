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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Ensures that the item the directive is acting on/against is valid for the directive as its defined in the target schema.
    /// </summary>
    internal class Rule_5_7_2_DirectiveMustBeUsedInValidLocation
        : DocumentPartValidationRuleStep<IDirectiveDocumentPart>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            var queryDirective = context.FindContextItem<IDirectiveDocumentPart>();
            if (queryDirective == null)
                return false;

            if (!queryDirective.Directive.Locations.HasFlag(queryDirective.Location))
            {
                var locationName = queryDirective.Location.ToString();
                var allowedLocations = queryDirective.Directive.Locations.GetIndividualFlags<DirectiveLocation>();

                var allowedString = string.Join(", ", allowedLocations.Select(x => x.ToString()));
                this.ValidationError(
                    context,
                    queryDirective.Node,
                    $"Invalid directive location. Attempted use of '{queryDirective.Directive.Name}' at location '{locationName}'. " +
                    $"Allowed Locations:  {allowedString}");
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.7.2";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Directives-Are-In-Valid-Locations";
    }
}