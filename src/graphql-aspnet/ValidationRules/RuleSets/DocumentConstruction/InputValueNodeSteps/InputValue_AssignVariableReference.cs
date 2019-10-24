// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputValueNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Assigns a <see cref="QueryInputArgument"/> to the current node context for the active node.
    /// </summary>
    internal class InputValue_AssignVariableReference
        : DocumentConstructionRuleStep<VariableValueNode, QueryOperation>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.FindContextItem<QueryInputValue>() is QueryVariableReferenceInputValue;
        }

        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (VariableValueNode)context.ActiveNode;
            var queryOperation = context.FindContextItem<QueryOperation>();
            var queryValue = context.FindContextItem<QueryInputValue>() as QueryVariableReferenceInputValue;

            var variable = queryOperation.Variables[node.Value.ToString()];
            variable.MarkAsReferenced();
            queryValue?.AssignVariableReference(variable);
            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.8.3";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-All-Variable-Uses-Defined";
    }
}