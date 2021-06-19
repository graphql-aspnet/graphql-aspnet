// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.NamedFragmentNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// <para>(5.5.1.1) Validate that each named fragment has a unique name within the document scope.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/June2018/#sec-Fragment-Name-Uniqueness .</para>
    /// </summary>
    internal class Rule_5_5_1_1_FragmentNamesMustBeUnique : DocumentConstructionRuleStep<NamedFragmentNode>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode"/> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var namedFragment = (NamedFragmentNode)context.ActiveNode;
            if (context.DocumentContext.Fragments.ContainsKey(namedFragment.FragmentName.ToString()))
            {
                this.ValidationError(
                    context,
                    $"Duplicate Fragment Name. The name '{namedFragment.FragmentName.ToString()}' must be unique in this document. Ensure that each " +
                    "fragment in the document is unique (case-sensitive).");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.1.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Fragment-Name-Uniqueness";
    }
}