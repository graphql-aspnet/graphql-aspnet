// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.DirectiveNodeSteps
{
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that the item the directive is acting on/against is valid for the directive as its defined in the target schema.
    /// </summary>
    internal class Rule_5_7_2_DirectiveMustBeUsedInValidLocation : DocumentConstructionRuleStep<DirectiveNode, QueryDirective>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (DirectiveNode)context.ActiveNode;
            var queryDirective = context.FindContextItem<QueryDirective>();

            if (queryDirective == null)
                return false;

            var location = node.ParentNode?.DirectiveLocation();
            if (!location.HasValue || !queryDirective.Directive.Locations.HasFlag(location))
            {
                var locationName = location.HasValue ? location.Value.ToString() : "unknown";
                var allowedLocations = queryDirective.Directive.Locations.GetIndividualFlags<DirectiveLocation>();

                var allowedString = string.Join(", ", allowedLocations.Select(x => x.ToString()));
                this.ValidationError(
                    context,
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