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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A rule that dictates each variables core type must be a SCALAR, ENUM or INPUT_OBJECT.
    /// </summary>
    internal class Rule_5_8_2_C_VariableGraphTypeMustBeOfAllowedTypeKinds : DocumentConstructionRuleStep<QueryVariable>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var variable = context.FindContextItem<QueryVariable>();
            var kind = variable.GraphType?.Kind ?? TypeKind.NONE;
            switch (kind)
            {
                case TypeKind.SCALAR:
                case TypeKind.ENUM:
                case TypeKind.INPUT_OBJECT:
                    return true;

                default:
                    var errorName = variable.GraphType?.Name ?? "{null}";
                    var errorKind = variable.GraphType == null ? string.Empty : ", which is of kind  '{variable.GraphType.Kind.ToString()}'";
                    this.ValidationError(
                        context,
                        $"Invalid Variable Graph Type. The variable named '${variable.Name}' references the graph type " +
                        $"'{errorName}'{errorKind}.  Only " +
                        $"{TypeKind.SCALAR.ToString()}, {TypeKind.ENUM.ToString()} and '{TypeKind.INPUT_OBJECT.ToString()}' are allowed for " +
                        "variable declarations.");

                    return false;
            }
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.8.2";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Variables-Are-Input-Types";
    }
}