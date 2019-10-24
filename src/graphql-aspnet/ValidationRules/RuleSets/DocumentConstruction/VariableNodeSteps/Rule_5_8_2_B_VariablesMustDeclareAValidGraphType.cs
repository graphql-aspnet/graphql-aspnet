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
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// A rule that dictates each variables core type must be a SCALAR, ENUM or INPUT_OBJECT.
    /// </summary>
    internal class Rule_5_8_2_B_VariablesMustDeclareAValidGraphType : DocumentConstructionRuleStep<QueryVariable>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var variable = context.FindContextItem<QueryVariable>();

            var graphType = context.DocumentContext.Schema.KnownTypes.FindGraphType(variable.TypeExpression.TypeName);
            if (graphType == null)
            {
                this.ValidationError(
                    context,
                    $"Unknown Variable Graph Type. The variable named '{variable?.Name}' declares " +
                    $"itself as a graph type of '{variable.TypeExpression}' but that graph type does not " +
                    "exist in the schema.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.8.2";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Variables-Are-Input-Types";
    }
}