// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.TopLevelNodeSteps
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// <para>(5.1.1) Verify that all top level definitions in the document are either an operation definition
    /// or a fragment definition.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/June2018/#sec-Executable-Definitions .</para>
    /// </summary>
    internal class Rule_5_1_1_ExecutableDefinitions : DocumentConstructionRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            // top level nodes only
            return context.ActiveNode != null && context.ActiveNode.ParentNode == null;
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode"/> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            if (context.ActiveNode is NamedFragmentNode)
                return true;

            if (context.ActiveNode is OperationTypeNode otn)
            {
                var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(otn.OperationType.ToString());

                if (operationType == GraphCollection.Unknown)
                {
                    this.ValidationError(
                        context,
                        $"Invalid Executable Definition. Expected executable definition of type '{nameof(OperationTypeNode)}' " +
                        $"or '{nameof(NamedFragmentNode)}' but recieved '{otn.OperationName.ToString()}'.");

                    return false;
                }

                return true;
            }

            this.ValidationError(
                context,
                $"Invalid Executable Definition. Expected executable definition of type '{nameof(OperationTypeNode)}' " +
                $"or '{nameof(NamedFragmentNode)}' but recieved '{context.ActiveNode?.GetType().FriendlyName() ?? "-nothing-"}'.");

            return false;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.1.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Executable-Definitions";
    }
}