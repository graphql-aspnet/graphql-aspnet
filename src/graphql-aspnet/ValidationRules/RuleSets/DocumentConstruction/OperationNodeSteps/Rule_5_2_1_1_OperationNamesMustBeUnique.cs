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
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// <para>(5.2.1.1) Validate that each top level operation has a unique name within the document scope.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/June2018/#sec-Operation-Name-Uniqueness" .</para>
    /// </summary>
    internal class Rule_5_2_1_1_OperationNamesMustBeUnique : DocumentConstructionRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return context.ActiveNode is OperationTypeNode operation && !operation.OperationName.IsEmpty;
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode"/> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (OperationTypeNode)context.ActiveNode;
            if (context.DocumentContext.Operations.ContainsKey(node.OperationName.ToString()))
            {
                var operationName = node.OperationName.IsEmpty ? "{anonymous}" : node.OperationName.ToString();
                this.ValidationError(
                    context,
                    $"Duplicate Operation Name. The operation named '{operationName}' must be unique " +
                    "in this document. Ensure that each query has a unique name (case-sensitive).");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.2.1.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Operation-Name-Uniqueness";
    }
}