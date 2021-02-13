// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.OperationNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Generates the <see cref="QueryOperation"/> to representing the operation node.
    /// </summary>
    internal class Rule_5_2_OperationTypeMustBeDefinedOnTheSchema : DocumentConstructionRuleStep<OperationTypeNode>
    {
        /// <summary>
        /// Determines where this context is in a state such that it should continue processing its children. Returning
        /// false will cease processing child nodes under the active node of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other nodes in the tree. This step is always executed
        /// even if the primary execution is skipped.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child node rulesets should be executed, <c>false</c> otherwise.</returns>
        public override bool ShouldAllowChildContextsToExecute(DocumentConstructionContext context)
        {
            // if we cant determine the operation type we cant deteremine the root graph type
            // and cant effectively parse the query document.
            var node = (OperationTypeNode)context.ActiveNode;

            var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(node.OperationType.ToString());
            if (!context.DocumentContext.Schema.OperationTypes.ContainsKey(operationType))
                return false;

            return true;
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (OperationTypeNode)context.ActiveNode;

            var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(node.OperationType.ToString());
            if (!context.DocumentContext.Schema.OperationTypes.ContainsKey(operationType))
            {
                this.ValidationError(
                    context,
                    $"The target schema does not contain a '{node.OperationType.ToString()}' operation type.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.2";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Validation.Operations";
    }
}