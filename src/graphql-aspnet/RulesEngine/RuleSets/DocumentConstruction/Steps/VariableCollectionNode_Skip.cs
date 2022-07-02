// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Steps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// A step to pass through the <see cref="VariableCollectionNode"/> effectively
    /// allowing the attachment of variables directly to their assigned operation.
    /// </summary>
    internal class VariableCollectionNode_Skip
        : DocumentConstructionStep<VariableCollectionNode>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            context.Skip();
            return true;
        }
    }
}