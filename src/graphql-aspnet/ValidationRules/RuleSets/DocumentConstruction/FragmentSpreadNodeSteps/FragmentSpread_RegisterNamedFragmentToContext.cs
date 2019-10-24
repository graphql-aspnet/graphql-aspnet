// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FragmentSpreadNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Assigns the active query fragment on the current context to be the fragment pointed to by the spread.
    /// </summary>
    internal class FragmentSpread_RegisterNamedFragmentToContext : DocumentConstructionStep<FragmentSpreadNode>
    {
        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FragmentSpreadNode)context.ActiveNode;
            QueryFragment fragment = context.DocumentContext.Fragments.FindFragment(node.PointsToFragmentName.ToString());
            context.AddDocumentPart(fragment);

            fragment.MarkAsReferenced();

            context.AppendNodes(fragment.Node.Children);
            context.BeginNewDocumentScope();
            context.DocumentScope.RestrictFieldsToGraphType(fragment.GraphType);
            return true;
        }
    }
}