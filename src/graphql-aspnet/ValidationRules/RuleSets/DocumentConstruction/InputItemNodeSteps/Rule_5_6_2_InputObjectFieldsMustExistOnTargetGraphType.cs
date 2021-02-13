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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Ensures that any field parsed from an object literal actual exists on the INPUT_OBJECT and that no extra fields are
    /// provided.
    /// </summary>
    internal class Rule_5_6_2_InputObjectFieldsMustExistOnTargetGraphType : DocumentConstructionRuleStep<InputItemNode, QueryInputArgument>
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
                   context.ActiveNode.ParentNode?.ParentNode is ComplexValueNode;
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

            // represents a field on a complex object argument
            // ensures that the field exists in the graphtype of the argument
            if (!(argument.GraphType is IInputObjectGraphType inputGraphType) || !inputGraphType.Fields.ContainsKey(node.InputName.ToString()))
            {
                this.ValidationError(
                    context,
                    $"The {argument.GraphType.Kind.ToString()} type '{argument.GraphType.Name}' does not define a field named '{node.InputName.ToString()}'.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.6.2";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Input-Object-Field-Names";
    }
}