// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputItemCollectionNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// A rule that ensures a set of input items only exists on a directive or a field.
    /// </summary>
    internal class Rule_5_4_InputItemsOnlyOnFieldsOrDirectives : DocumentConstructionRuleStep<InputItemCollectionNode>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (InputItemCollectionNode)context.ActiveNode;

            if (!(node.ParentNode is FieldNode) && !(node.ParentNode is DirectiveNode) && !(node.ParentNode is ComplexValueNode))
            {
                this.ValidationError(
                    context,
                    $"The node, of type '{node.ParentNode?.GetType().Name ?? "-none-"}' cannot contain a collection of input arguments. " +
                    $"Input arguments are only valid on a '{typeof(FieldNode).Name}' and '{typeof(DirectiveNode).Name}' ");
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.4";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Validation.Arguments";
    }
}