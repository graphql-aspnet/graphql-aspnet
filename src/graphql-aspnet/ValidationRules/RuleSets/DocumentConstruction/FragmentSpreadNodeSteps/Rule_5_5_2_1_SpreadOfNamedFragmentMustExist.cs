// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FragmentSpreadNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Checks to ensure that when a spread of a named fragment occurs (<see cref="FragmentSpreadNode"/>) that the named
    /// fragment exists in the document.
    /// </summary>
    internal class Rule_5_5_2_1_SpreadOfNamedFragmentMustExist : DocumentConstructionRuleStep<FragmentSpreadNode>
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FragmentSpreadNode)context.ActiveNode;

            var targetFragment = node.PointsToFragmentName.ToString();
            if (context.DocumentContext.Fragments.ContainsKey(targetFragment))
                return true;

            this.ValidationError(
                context,
                $"The named fragment '{targetFragment}' does not exist in the supplied document.");

            return false;
        }

        /// <summary>
        /// Gets the rule number.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.2.1";

        /// <summary>
        /// Gets the reference URL.
        /// </summary>
        /// <value>The reference URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Fragment-spread-target-defined";
    }
}