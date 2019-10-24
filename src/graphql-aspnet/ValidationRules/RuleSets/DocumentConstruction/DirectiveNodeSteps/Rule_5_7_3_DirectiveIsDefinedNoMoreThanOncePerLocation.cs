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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Ensures that for hte location where this directive exists no other directive was parsed with the same name.
    /// </summary>
    internal class Rule_5_7_3_DirectiveIsDefinedNoMoreThanOncePerLocation : DocumentConstructionRuleStep<QueryDirective>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var queryDirective = context.FindContextItem<QueryDirective>();

            if (context.DocumentScope.Directives.Count(x =>
                    x.Directive.Name == queryDirective.Directive.Name &&
                    x.Directive.Locations.HasFlag(queryDirective.Location)) > 1)
            {
                this.ValidationError(
                    context,
                    $"The directive '{queryDirective.Name}' is already defined in this location. Directives must be unique per " +
                    "instantiated location (e.g. once per field, once per input argument etc.).");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.7.3";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Directives-Are-Unique-Per-Location";
    }
}