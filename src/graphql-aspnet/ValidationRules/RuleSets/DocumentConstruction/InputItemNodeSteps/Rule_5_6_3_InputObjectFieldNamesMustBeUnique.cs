// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputItemNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Ensures that there are no duplicate fields provided on a complex input object.
    /// </summary>
    internal class Rule_5_6_3_InputObjectFieldNamesMustBeUnique : DocumentConstructionRuleStep<InputItemNode, QueryInputArgument>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the input argument if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified input argument; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) &&
                   context.ActiveNode.ParentNode?.ParentNode is ComplexValueNode &&
                   context.FindContextItem<QueryInputValue>() is QueryComplexInputValue;
        }

        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (InputItemNode)context.ActiveNode;
            var argument = context.FindContextItem<QueryInputArgument>();
            var complexValue = context.FindContextItem<QueryInputValue>() as QueryComplexInputValue;

            if (complexValue.Arguments.ContainsKey(node.InputName.ToString()))
            {
                this.ValidationError(
                    context,
                    $"Fields on input objects must be unique. The supplied value for argument '{argument.Name}' " +
                    $"defines '{node.InputName.ToString()}' more than once.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.6.3";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Input-Object-Field-Uniqueness";
    }
}