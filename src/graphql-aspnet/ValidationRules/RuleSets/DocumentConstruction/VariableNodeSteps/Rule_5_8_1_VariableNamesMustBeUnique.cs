// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.VariableNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// A rule that dictates that for any set of variables all the names must be unique.
    /// </summary>
    internal class Rule_5_8_1_VariableNamesMustBeUnique : DocumentConstructionRuleStep<VariableNode, QueryOperation>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (VariableNode)context.ActiveNode;
            var operation = context.FindContextItem<QueryOperation>();

            if (operation?.Variables != null)
            {
                if (operation.Variables.ContainsKey(node.Name.ToString()))
                {
                    this.ValidationError(
                        context,
                        $"Duplicate Variable Name. The variable named '{node.Name.ToString()}' must be unique " +
                        "in its contained operation. Ensure that all variable names, per operation, are unique (case-sensitive).");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.8.1";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Variable-Uniqueness";
    }
}