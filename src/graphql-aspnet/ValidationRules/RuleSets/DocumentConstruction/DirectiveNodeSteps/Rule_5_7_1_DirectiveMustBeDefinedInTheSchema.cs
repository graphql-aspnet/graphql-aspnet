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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A rule that checks a directive node's parent to ensure that the location the directive is attached to
    /// is valid based on the directive definition in the target schema.
    /// </summary>
    internal class Rule_5_7_1_DirectiveMustBeDefinedInTheSchema : DocumentConstructionRuleStep<DirectiveNode>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (DirectiveNode)context.ActiveNode;
            var directive = context.DocumentContext.Schema.KnownTypes.FindDirective(node.DirectiveName.ToString());
            if (directive == null || directive.Kind != TypeKind.DIRECTIVE)
            {
                this.ValidationError(
                    context,
                    $"The target schema does not contain a directive named '{node.DirectiveName.ToString()}'.");
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.7.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Directives-Are-Defined";
    }
}