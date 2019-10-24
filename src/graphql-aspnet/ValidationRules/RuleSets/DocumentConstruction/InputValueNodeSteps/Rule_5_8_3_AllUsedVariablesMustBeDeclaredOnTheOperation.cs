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
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Any input argument referencing a variable must have that variable declared on the local operation.
    /// </summary>
    internal class Rule_5_8_3_AllUsedVariablesMustBeDeclaredOnTheOperation
        : DocumentConstructionRuleStep<VariableValueNode, QueryOperation>
    {
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

            var variableName = node.Value.ToString();
            if (queryOperation.Variables == null || !queryOperation.Variables.ContainsKey(variableName))
            {
                this.ValidationError(
                    context,
                    $"The variable named '${variableName}' is not declared for the current operation.");

                return false;
            }

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